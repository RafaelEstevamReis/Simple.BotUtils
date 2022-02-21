using System;

namespace Simple.BotUtils.Jobs
{
    public class TaskErrorEventArgs : EventArgs
    {
        public JobInfo Info { get; }
        public Exception Exception { get; }

        public TaskErrorEventArgs(JobInfo info, Exception ex)
        {
            Info = info;
            Exception = ex;
        }

    }
}
