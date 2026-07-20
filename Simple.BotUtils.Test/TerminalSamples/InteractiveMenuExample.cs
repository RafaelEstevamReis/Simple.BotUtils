using Simple.BotUtils.Terminal;
using System;

namespace TerminalSamples
{
    public class InteractiveMenuExample
    {
        public static void ProgramMain(string[] args)
        {
            var iMnu = new InteractiveMenu()
                .SetCaption("Test caption (arrows to move, Enter to pick, Esc to cancel)")
                .SetSelectedBgColor(ConsoleColor.DarkGreen)
                .SetSelectedFgColor(ConsoleColor.White)
                .AddOption(new InteractiveMenu.MenuOption { Text = "Option 1" })
                .AddOption(new InteractiveMenu.MenuOption { Text = "Option 2", HotKey = ConsoleKey.F4, Description = "with a hotkey" })
                .AddOption(new InteractiveMenu.MenuOption { Text = "Disabled option", Enabled = false })
                .AddOption("Option 4 (quick action)", () => Console.WriteLine("action ran!"))
                ;

            // Prove the 9-option limit is gone
            for (int i = 5; i <= 15; i++)
            {
                iMnu.AddOption($"Option {i}");
            }

            var chosen = iMnu.Run();
            Console.WriteLine(chosen == null ? "Canceled" : $"Selected: {chosen.Text}");
        }
    }
}
