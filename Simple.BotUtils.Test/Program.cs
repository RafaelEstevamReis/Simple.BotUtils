﻿using Simple.BotUtils;

var menu = new Simple.BotUtils.Terminal
    .SimpleMenu()
    .SetCaption("Choose the sample")
    .AddOption("Configuration Example")
    .AddOption("Controller Example")
    .AddOption("Schedule Example")
    .AddOption("Interactive Menu Example")
    .AddOption("Controller with Authorization Example")
    .AddOption("BotBuilder")
    ;

var opt = menu.ShowMenu(true);

System.Console.Clear();
System.Console.WriteLine("## " + menu.Options[opt] + " ##");
System.Console.WriteLine();

switch (opt)
{
    case 0:
        ConfigurationSample.FullExample.ProgramMain(args);
        break;
    case 1:
        ControllerSample.FullExample.ProgramMain(args);
        break;
    case 2:
        ScheduleSample.FullExample.ProgramMain(args);
        break;

    case 3:
        TerminalSamples.InteractiveMenuExample.ProgramMain(args);
        break;

    case 4:
        ControllerSample.AuthorizedControllers.ProgramMain(args);
        break;

    case 5:
        BotBuilderSample.FullExample.ProgramMain(args);
        break;
}

