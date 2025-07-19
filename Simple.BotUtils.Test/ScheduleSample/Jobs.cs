using Simple.BotUtils.Jobs;
using Simple.BotUtils.Logging;
using System;
using System.Threading.Tasks;

namespace Simple.BotUtils.Test.ScheduleSample
{
    public class PingJob : JobBase
    {
        private readonly ILogger logger;

        public PingJob(ILogger logger)
        {
            CanBeScheduled = true;
            RunOnStartUp = true;
            CanBeInvoked = true;
            StartEvery = TimeSpan.FromSeconds(5);
            this.logger = logger;
        }

        public override async Task ExecuteAsync(ExecutionTrigger trigger, object parameter)
        {
            Console.WriteLine($"[{trigger}] Execute Job PING");

            await Task.CompletedTask;
        }
    }
}
