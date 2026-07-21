namespace Simple.BotUtils.Jobs;

using System;

public class TaskErrorEventArgs : EventArgs
{
    public JobInfo Info { get; }
    public Exception Exception { get; }

    public TaskErrorEventArgs(JobInfo info, Exception ex)
    {
        Info = info;
        Exception = ex;
    }

    public override string ToString() => $"[{Info.SchedulerJob.GetType().Name}] ERR {Exception}";
}
