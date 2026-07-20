using System;
using System.Threading.Tasks;

namespace Simple.BotUtils.Jobs
{
    public class JobInfo(IJob schedulerJob, Task? systemTask, DateTime lastExecution)
    {
        public IJob SchedulerJob { get; set; } = schedulerJob;
        public Task? SystemTask { get; set; } = systemTask;
        public DateTime LastExecution { get; set; } = lastExecution;

        public bool CanRun => SystemTask == null || SystemTask.IsCompleted;
    }
}
