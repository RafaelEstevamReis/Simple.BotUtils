using Simple.BotUtils.Data;
using System;

namespace Simple.BotUtils.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Messages.Mediator.Register(new Pinger());

            string input = Console.ReadLine();
            var output = Messages.Mediator.Execute<Pinger, string, string>(input);
            Console.WriteLine(output);

            // Create fake args
            args = new string[] { "-a", "nothing", "-n", "da-bot", "--MyLongNumber", "684261" };

            // load my config from Disk
            var cfg = ConfigBase.Load<MyConfig>("cfg.xml");

            // Apply args
            Startup.ArgumentParser.ParseInto(args, cfg);
            cfg.Save();

            // continue ...
            Console.WriteLine(cfg);
        }
    }

    public class Pinger : Messages.IMessenger<string, string>
    {
        public string Send(string request)
        {
            return $"pong: {request}";
        }
    }

    public class MyConfig : ConfigBase
    {
        [Startup.ArgumentKey("-n", "--name")]
        public string MyName { get; set; }
        [Startup.ArgumentKey("-i", "--info")]
        public string MyInfo { get; set; }
        [Startup.ArgumentKey("-nb", "--number")]
        public int MyNumber { get; set; }
        [Startup.ArgumentKey("-ln", "--long")]
        public long MyLongNumber { get; set; }

        public override string ToString()
        {
            return $"{MyName} | {MyInfo} | {MyNumber}: {MyLongNumber}";
        }
    }

}
