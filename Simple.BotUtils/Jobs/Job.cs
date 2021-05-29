using System;
using System.Threading.Tasks;

namespace Simple.BotUtils.Jobs
{
    public class Job : IJob
    {
        protected Job() { }
        public Job(Action<ExecutionTrigger, object> runAction,
                   bool runOnStartUp = false,
                   TimeSpan? startEvery = null,
                   bool canBeInvoked = false)
        {
            RunAction = runAction ?? throw new ArgumentNullException(nameof(runAction));

            CanBeInvoked = canBeInvoked;
            RunOnStartUp = runOnStartUp;
            if (startEvery.HasValue)
            {
                CanBeScheduled = true;
                StartEvery = startEvery.Value;
            }
            else
            {
                CanBeScheduled = false;
            }
        }

        public bool CanBeInvoked { get; set; }
        public bool CanBeScheduled { get; set; }
        public bool RunOnStartUp { get; set; }
        public TimeSpan StartEvery { get; set; }

        public Action<ExecutionTrigger, object> RunAction { get; }

#if NET40
        public virtual Task ExecuteAsync(ExecutionTrigger trigger, object parameter)
        {
            return Task.Factory.StartNew(() => RunAction(trigger, parameter));
        }
#else
        public virtual async Task ExecuteAsync(ExecutionTrigger trigger, object parameter)
        {
            await Task.Run(() => RunAction(trigger, parameter));
        }
#endif
    }
}
