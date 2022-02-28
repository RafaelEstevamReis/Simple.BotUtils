using System;

namespace Simple.BotUtils.Test.TerminalSamples
{
    public class InteractiveMenuExample
    {
        public static void ProgramMain(string[] args)
        {
            var iMnu = new Terminal
                .InteractiveMenu()
                .SetCaption("Test caption")
                .SetSelectedBgColor(ConsoleColor.DarkGreen)
                .SetSelectedFgColor(ConsoleColor.White)
                .AddOption(new Terminal.InteractiveMenu.MenuOption() { Text = "Option 1" })
                .AddOption(new Terminal.InteractiveMenu.MenuOption() { Text = "Option 2", HotKey = ConsoleKey.F4 })
                .AddOption(new Terminal.InteractiveMenu.MenuOption() { Text = "Option 3" })
                .AddOption(new Terminal.InteractiveMenu.MenuOption() { Text = "Option 4", HotKey = ConsoleKey.Escape })
                ;
            var opt2 = iMnu.ShowMenu();
            Console.WriteLine(opt2);

        }
    }
}
