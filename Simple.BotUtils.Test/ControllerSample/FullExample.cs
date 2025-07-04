﻿using Simple.BotUtils.Controllers;
using Simple.BotUtils.Test;
using Simple.BotUtils.Test.ControllerSample;
using System;

namespace ControllerSample
{
    public class FullExample
    {
        public static void ProgramMain(string[] args)
        {
            // Inject config for later use
            var cfg = Simple.BotUtils.Test.MyConfig.Load("cfg.json");
            Simple.BotUtils.DI.Injector.AddSingleton(cfg);

            // Call Methods
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
            ctrl.ExecuteFromText(context: new Message { Content = message }, "ShowInfoObject \"Bla bla bla bla\"");


            // Support for `params`
            ctrl.ExecuteFromText("echo a b c d e");

            // Controllers with constructors are instantiated from DI
            var ctorCtrl = new ControllerManager()
                       .AddController<ControllersWithJob>()
                       .AddController<AdvancedControllers>();
            ctorCtrl.Filter += (s, ev) => { };

            ctorCtrl.Execute("ShowMyName");
            // Support Slash commands
            ctorCtrl.AcceptSlashInMethodName = true;
            ctorCtrl.Execute("/ShowMyName");
            // Tasks ignore Async name
            ctorCtrl.Execute("DoTask");

            // Alias method
            ctorCtrl.Execute("little_1");
            ctorCtrl.Execute("big_1");

            // CallerInfo aware methods
            ctorCtrl.Execute("SelfAware");
        }

    }
}
