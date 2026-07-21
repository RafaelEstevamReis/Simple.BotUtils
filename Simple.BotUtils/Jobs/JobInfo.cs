namespace Simple.BotUtils.Jobs;

using System;
using System.Threading.Tasks;

public class JobInfo(IJob schedulerJob, Task? systemTask, DateTime lastExecution)
{
    public IJob SchedulerJob { get; set; } = schedulerJob;
    public Task? SystemTask { get; set; } = systemTask;
    public DateTime LastExecution { get; set; } = lastExecution;

    public bool CanRun => SystemTask == null || SystemTask.IsCompleted;
}
