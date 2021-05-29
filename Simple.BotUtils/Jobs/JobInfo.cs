using System;
using System.Threading.Tasks;

namespace Simple.BotUtils.Jobs
{
    public class JobInfo
    {
        public IJob SchedulerJob { get; set; }
        public Task SystemTask { get; set; }
        public DateTime LastExecution { get; set; }

        public bool CanRun => SystemTask == null || SystemTask.IsCompleted;
    }
}
