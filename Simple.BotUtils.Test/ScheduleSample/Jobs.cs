using Simple.BotUtils.Jobs;
using System;
using System.Threading.Tasks;

namespace Simple.BotUtils.Test.ScheduleSample
{
    public class PingJob : JobBase
    {
        public PingJob()
        {
            CanBeScheduled = true;
            RunOnStartUp = true;
            CanBeInvoked = true;
            StartEvery = TimeSpan.FromSeconds(5);
        }

        public override async Task ExecuteAsync(ExecutionTrigger trigger, object parameter)
        {
            Console.WriteLine($"Execute Job: {trigger}");

            if (trigger == ExecutionTrigger.Scheduled) throw new NotSupportedException();

            return;
        }
    }
}
