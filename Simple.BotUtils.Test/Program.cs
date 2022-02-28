var menu = new Simple.BotUtils.Terminal
    .SimpleMenu()
    .SetCaption("Choose the sample")
    .AddOption("Configuration Example")
    .AddOption("Controller Example")
    .AddOption("Schedule Example")
    .AddOption("Interactive Menu Example")
    ;

var opt = menu.ShowMenu(true);
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
}

