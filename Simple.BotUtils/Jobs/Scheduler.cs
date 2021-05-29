using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.BotUtils.Jobs
{
    public class Scheduler
    {
        readonly Dictionary<Type, JobInfo> jobs;

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

        public void RunStartJobs()
        {
            foreach (var v in jobs.Values)
            {
                if (!v.CanRun) continue;
                if (!v.SchedulerJob.RunOnStartUp) continue;

                v.SystemTask = v.SchedulerJob.ExecuteAsync(ExecutionTrigger.Startup, null);
                v.LastExecution = DateTime.Now;
            }
        }
        [Obsolete("Use RunTimedJobs instead")]
        public void RunTimedJob() => RunTimedJobs();
        public void RunTimedJobs()
        {
            foreach (var v in jobs.Values)
            {
                if (!v.CanRun) continue;
                if (!v.SchedulerJob.CanBeScheduled) continue;

                var time = DateTime.Now - v.LastExecution;
                if (time < v.SchedulerJob.StartEvery) continue;

                v.SystemTask = v.SchedulerJob.ExecuteAsync(ExecutionTrigger.Startup, null);
                v.LastExecution = DateTime.Now;
            }
        }
        public bool RunJob<T>(object parameter)
        {
            var t = typeof(T);
            if (!jobs.TryGetValue(t, out JobInfo info)) return false;


            if (!info.CanRun) return false;
            if (!info.SchedulerJob.CanBeInvoked) return false;

            info.SystemTask = info.SchedulerJob.ExecuteAsync(ExecutionTrigger.Invoked, parameter);
            info.LastExecution = DateTime.Now;

            return true;
        }
        /// <summary>
        /// Runs Synchronously calling RunTimedJobs() every 10 seconds
        /// </summary>
        public void RunJobsSynchronously(CancellationToken token)
        {
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
