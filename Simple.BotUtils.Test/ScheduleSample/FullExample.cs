using Simple.BotUtils.Jobs;
using Simple.BotUtils.Test.ScheduleSample;
using System;
using System.Threading;

namespace ScheduleSample
{
    public class FullExample
    {
        public static void ProgramMain(string[] args)
        {
            // Process tasks
            var cancellationSource = new CancellationTokenSource();
            var scheduler = new Scheduler();
            scheduler.Error += (s, err) => Console.WriteLine($"Error on [{err.Info}]: {err.Exception.Message}");
            scheduler.Add(new PingJob());

            scheduler.RunJobsSynchronously(cancellationSource.Token);
        }
    }
}
