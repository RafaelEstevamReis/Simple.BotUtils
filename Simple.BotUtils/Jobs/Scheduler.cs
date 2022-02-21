using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.BotUtils.Jobs
{
    public class Scheduler
    {
        readonly Dictionary<Type, JobInfo> jobs;

        public event EventHandler<TaskErrorEventArgs> Error;

        public Scheduler()
        {
            jobs = new Dictionary<Type, JobInfo>();
        }

        public Scheduler Add<T>(T job) where T : IJob
        {
            return Add(typeof(T), job);
        }
        public Scheduler Add(Type t, IJob task)
        {
            jobs[t] = new JobInfo()
            {
                SchedulerJob = task,
                SystemTask = null
            };
            return this;
        }

        public int JobCount => jobs.Count;

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
            foreach (var v in jobs.Values)
            {
                if (!v.CanRun) continue;
                if (!v.SchedulerJob.CanBeScheduled) continue;

                var time = DateTime.Now - v.LastExecution;
                if (time < v.SchedulerJob.StartEvery) continue;

                runJob(v, ExecutionTrigger.Scheduled, null);
            }
        }
        public bool RunJob<T>(object parameter)
        {
            var t = typeof(T);
            if (!jobs.TryGetValue(t, out JobInfo info)) return false;


            if (!info.CanRun) return false;
            if (!info.SchedulerJob.CanBeInvoked) return false;

            runJob(info, ExecutionTrigger.Invoked, parameter);

            return true;
        }

        private void runJob(JobInfo info, ExecutionTrigger trigger, object parameter)
        {
            var task = info.SchedulerJob.ExecuteAsync(trigger, parameter);
            info.LastExecution = DateTime.Now;
            info.SystemTask = task;

            collectTaskErrors(info, task);
        }
        private void collectTaskErrors(JobInfo info, Task task)
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
                int timeDelaySeconds = 10; // 10s
#if NET40
                for (int i = 0; i < timeDelaySeconds; i++)
                {
                    Thread.Sleep(1000);
                    if (token.IsCancellationRequested) break;
                }
#else
                Task.Delay(timeDelaySeconds * 10, token).Wait();
#endif
                if (token.IsCancellationRequested) break;
                RunTimedJobs();
            }
        }

        public JobInfo GetJobInfo<T>()
        {
            var t = typeof(T);
            if (!jobs.TryGetValue(t, out JobInfo info)) return null;
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
