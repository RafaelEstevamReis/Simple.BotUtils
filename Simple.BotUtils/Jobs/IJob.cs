using System;
using System.Threading.Tasks;

namespace Simple.BotUtils.Jobs
{
    public interface IJob
    {
        bool CanBeInvoked { get; }
        bool CanBeScheduled { get; }
        bool RunOnStartUp { get; }
        TimeSpan StartEvery { get; }
        Task ExecuteAsync(ExecutionTrigger trigger, object parameter);
    }
}
