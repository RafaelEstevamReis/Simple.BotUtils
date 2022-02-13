using Simple.BotUtils.Controllers;
using Simple.BotUtils.Data;
using Simple.BotUtils.DI;
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
            // Save Config in DI
            Injector.AddSingleton(cfg);

            // continue ...
            Console.WriteLine(cfg);

            // Call Mathods
            var ctrl = new ControllerManager()
                       .AddController<MyControllers>();
            //          OR
            //         .AddControllers(System.Reflection.Assembly.GetExecutingAssembly());

            // Void methods
            ctrl.Execute("ShowInfo", "Bla bla bla bla");
            ctrl.Execute("ShowNumber", "42");
            ctrl.Execute("ShowDouble", "42.42"); // string
            ctrl.Execute("ShowDouble", 42.42); // Native
            // Return methods
            int sum = ctrl.Execute<int>("Sum", "40", "2"); // string
            sum = ctrl.Execute<int>("Sum", 40, 2); // Native
            Console.WriteLine($"Sum: {sum}");
            // Using DI injection, ShowMyName will get cfg object from DI
            ctrl.Execute("ShowMyName");

            // splitting a received text, passing a context argument
            string message = "ShowCallerInfo \"Bla bla bla bla\"";
            ctrl.ExecuteFromText(context: 42, text: message);
            // Support for `params`
            ctrl.ExecuteFromText("echo a b c d e");
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

    public class MyControllers : IController
    {
        public void ShowInfo(string info) => Console.WriteLine(info);
        public void ShowNumber(int number) => Console.WriteLine(number);
        public void ShowDouble(double number) => Console.WriteLine(number);
        public int Sum(int a, int b) => a + b;
        public void ShowMyName([FromDI] MyConfig cfg) => Console.WriteLine(cfg.MyName);

        public void ShowCallerInfo(int contextParam, string textParams, [FromDI] MyConfig cfg)
            => Console.WriteLine($"ShowCallerInfo[{contextParam}] {textParams}");

        public void Echo(params string[] contents)
        {
            Console.WriteLine(string.Join(' ', contents));
        }
    }

}
