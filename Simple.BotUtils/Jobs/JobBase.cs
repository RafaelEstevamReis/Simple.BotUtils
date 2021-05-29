using System;
using System.Threading.Tasks;

namespace Simple.BotUtils.Jobs
{
    public abstract class JobBase : IJob
    {
        protected JobBase() { RunAction = null; }
        protected JobBase(Action<ExecutionTrigger, object> runAction)
        {
            RunAction = runAction;
        }

        public bool CanBeInvoked { get; set; }
        public bool CanBeScheduled { get; set; }
        public bool RunOnStartUp { get; set; }
        public TimeSpan StartEvery { get; set; }

        public Action<ExecutionTrigger, object> RunAction { get; }

#if NET40
        public virtual Task ExecuteAsync(ExecutionTrigger trigger, object parameter)
        {
            if (RunAction == null) throw new Exception("You must either supply a RunAction or Override ExecuteAsync");
            return Task.Factory.StartNew(() => RunAction(trigger, parameter));
        }
#else
        public virtual async Task ExecuteAsync(ExecutionTrigger trigger, object parameter)
        {
            if (RunAction == null) throw new Exception("You must either supply a RunAction or Override ExecuteAsync");
            await Task.Run(() => RunAction(trigger, parameter));
        }
#endif
    }
}
