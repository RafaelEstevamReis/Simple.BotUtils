using Simple.BotUtils.Data;
using System;

namespace Simple.BotUtils.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // Create fake args
            args = new string[] { "-a", "nothing", "-n", "da-bot", "--MyLongNumber", "684261", "--MyNegativeNumber", "\"-123456\"" };

            // load my config from Disk
            var cfg = MyConfig.Load("cfg.xml");
            
            // Apply args
            Startup.ArgumentParser.ParseInto(args, cfg);
            cfg.Save();

            // continue ...
            Console.WriteLine(cfg);
        }
    }

    public class MyConfig : ConfigBase<MyConfig>
    {
        [Startup.ArgumentKey("-n", "--name")]
        public string MyName { get; set; }
        [Startup.ArgumentKey("-i", "--info")]
        public string MyInfo { get; set; }
        [Startup.ArgumentKey("-nb", "--number")]
        public int MyNumber { get; set; }
        [Startup.ArgumentKey("-ln", "--long")]
        public long MyLongNumber { get; set; }
        [Startup.ArgumentKey("-nn", "--MyNegativeNumber")]
        public int MyNegativeNumber { get; set; }

        public override string ToString()
        {
            return $"{MyName} | {MyInfo} | {MyNumber}: {MyLongNumber}";
        }
    }

}
