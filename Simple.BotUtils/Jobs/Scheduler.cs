using Simple.BotUtils.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.BotUtils.Jobs
{
    public class Scheduler
    {
        private const int timeDelaySeconds = 10; // 10s
        readonly Dictionary<string, JobInfo> jobs;

        public event EventHandler<TaskErrorEventArgs> Error;

        public Scheduler()
        {
            jobs = new Dictionary<string, JobInfo>();
        }

        public Scheduler Add<T>(T job) where T : IJob
        {
            return Add(job.GetType(), job);
        }
        public Scheduler Add(Type t, IJob task)
        {
            jobs[t.FullName] = new JobInfo()
            {
                SchedulerJob = task,
                SystemTask = null
            };
            return this;
        }

        public int JobCount => jobs.Count;

        public IEnumerable<string> GetRegisteredTypes()
        {
            // Ensures the enumeration completition before return
            return jobs.Keys.ToArray();
        }
        public IEnumerable<JobInfo> GetRegisteredJobs()
        {
            // Ensures the enumeration completition before return
            return jobs.Values.ToArray();
        }

        bool hasStartExecuted;
        public void RunStartJobs()
        {
            hasStartExecuted = true;
            foreach (var v in jobs.Values)
            {
                if (!v.CanRun) continue;
                if (!v.SchedulerJob.RunOnStartUp) continue;

                runJob(v, ExecutionTrigger.Startup, null);
            }
        }

        public void RunTimedJobs()
        {
            List<Exception> lstex = new List<Exception>();
            foreach (var v in jobs.Values)
            {
                if (!v.CanRun) continue;
                if (!v.SchedulerJob.CanBeScheduled) continue;

                var time = DateTime.Now - v.LastExecution;
                if (time < v.SchedulerJob.StartEvery) continue;

                try
                {
                    runJob(v, ExecutionTrigger.Scheduled, null);
                }
                catch (Exception ex)
                {
                    lstex.Add(new Exception("Error while running job: " + v.SchedulerJob, ex));
                }
            }
            if (lstex.Count > 0) throw new AggregateException(lstex.ToArray());
        }

        public bool RunJob<T>(object parameter)
            => RunJob<T>(parameter, out string _);
        public bool RunJob<T>(object parameter, out string failedReason)
        {
            var t = typeof(T);
            if (!jobs.TryGetValue(t.FullName, out JobInfo info))
            {
                failedReason = "Type not present";
                return false;
            }

            return RunJob(info, parameter, out failedReason);
        }
        public bool RunJob(JobInfo info, object parameter)
            => RunJob(info, parameter, out string _);
        public bool RunJob(JobInfo info, object parameter, out string failedReason)
        {
            if (!info.CanRun)
            {
                string taskStatus = "";
                if (info.SystemTask != null)
                {
                    taskStatus = " Task: " + info.SystemTask.Status;
                    if (!info.SystemTask.IsCompleted) taskStatus += " [NotCompleted]";
                }

                failedReason = "Job can not Run" + taskStatus;
                return false;
            }
            if (!info.SchedulerJob.CanBeInvoked)
            {
                failedReason = "Job can not be invoked";
                return false;
            }

            runJob(info, ExecutionTrigger.Invoked, parameter);

            failedReason = null;
            return true;
        }

        private void runJob(JobInfo info, ExecutionTrigger trigger, object parameter)
        {
            var task = info.SchedulerJob.ExecuteAsync(trigger, parameter);
            info.LastExecution = DateTime.Now;
            info.SystemTask = task;

            waitAndCollectTaskErrors(info, task);
        }
        private void waitAndCollectTaskErrors(JobInfo info, Task task)
        {
            try
            {
                task.Wait();
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    raiseErrorEvent(new TaskErrorEventArgs(info, e));
                }
            }
            catch (Exception ex)
            {
                raiseErrorEvent(new TaskErrorEventArgs(info, ex));
            }
        }
        private void raiseErrorEvent(TaskErrorEventArgs taskErrorEventArgs)
        {
            Error?.Invoke(this, taskErrorEventArgs);
        }

        /// <summary>
        /// Runs Synchronously calling RunTimedJobs() every 10 seconds
        /// </summary>
        public void RunJobsSynchronously(CancellationToken token)
        {
            if (!hasStartExecuted) RunStartJobs();

            while (true)
            {
#if NET40
                for (int i = 0; i < timeDelaySeconds * 10; i++)
                {
                    Thread.Sleep(100);
                    if (token.IsCancellationRequested) break;
                }
#else
                Task.Delay(timeDelaySeconds * 10, token).Wait(); // Do not throw exception
#endif
                if (token.IsCancellationRequested) break;
                RunTimedJobs();
            }
        }

        public JobInfo GetJobInfo<T>()
        {
            var t = typeof(T);
            if (!jobs.TryGetValue(t.FullName, out JobInfo info)) return null;
            // Make a copy
            return new JobInfo()
            {
                LastExecution = info.LastExecution,
                SchedulerJob = info.SchedulerJob,
                SystemTask = info.SystemTask
            };
        }
    }
}
