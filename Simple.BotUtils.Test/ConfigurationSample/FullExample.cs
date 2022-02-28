using Simple.BotUtils.Data;
using Simple.BotUtils.DI;
using Simple.BotUtils.Startup;
using Simple.BotUtils.Test;
using System;

namespace ConfigurationSample
{
    public class FullExample
    {
        public static void ProgramMain(string[] args)
        {
            Console.WriteLine("Hello World!");

            // Create fake args
            args = new string[] { "-a", "nothing", "-n", "da-bot", "--MyLongNumber", "684261", "--MyNegativeNumber", "\"-123456\"" };

            // load my config from Disk
            var cfg = MyConfig.Load("cfg.xml");

            // Apply args
            ArgumentParser.ParseInto(args, cfg);
            cfg.Save();
            // Save Config in DI
            Injector.AddSingleton(cfg);

            // continue ...
            Console.WriteLine(cfg);

        }
    }

}
