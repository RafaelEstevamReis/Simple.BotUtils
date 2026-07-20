using Simple.BotUtils.Terminal;
using System;

var menu = new InteractiveMenu()
    .SetCaption("Choose the sample")
    .AddOption("Configuration Example", ConfigurationSample.FullExample.ProgramMain, args)
    .AddOption("Controller Example", ControllerSample.FullExample.ProgramMain, args)
    .AddOption("Schedule Example", ScheduleSample.FullExample.ProgramMain,args)
    .AddOption("Interactive Menu Example", TerminalSamples.InteractiveMenuExample.ProgramMain,args)
    .AddOption("Controller with Authorization Example", ControllerSample.AuthorizedControllers.ProgramMain, args)
    .AddOption("BotBuilder", BotBuilderSample.FullExample.ProgramMain, args)
    ;

var chosen = menu.Show();
if (chosen == null) return;

Console.Clear();
Console.WriteLine("## " + chosen.Text + " ##");
Console.WriteLine();

chosen.Action?.Invoke();
