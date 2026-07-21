namespace Simple.BotUtils.Jobs;

using System;
using System.Threading.Tasks;

public interface IJob
{
    bool CanBeInvoked { get; }
    bool CanBeScheduled { get; }
    bool RunOnStartUp { get; }
    TimeSpan StartEvery { get; }
    Task ExecuteAsync(ExecutionTrigger trigger, object? parameter);
}
