#if !NETSTANDARD1_0

using System;
using System.Collections.Generic;

namespace Simple.BotUtils.Terminal
{
    public class InteractiveMenu
    {
        public ConsoleColor ClearBgColor { get; set; } = ConsoleColor.Black;
        public ConsoleColor ClearFgColor { get; set; } = ConsoleColor.White;
        public ConsoleColor SelBgColor { get; set; } = ConsoleColor.White;
        public ConsoleColor SelFgColor { get; set; } = ConsoleColor.Black;

        public bool ShowOptionNumbers { get; set; } = true;
        public bool ShowOptionHotKeys { get; set; } = true;

        public string Caption { get; set; }
        public List<MenuOption> Options { get; private set; }

        public InteractiveMenu()
        {
            Options = new List<MenuOption>();
        }

        public InteractiveMenu SetClearBgColor(ConsoleColor color)
        {
            ClearBgColor = color;
            return this;
        }
        public InteractiveMenu SetClearFgColor(ConsoleColor color)
        {
            ClearFgColor = color;
            return this;
        }
        public InteractiveMenu SetSelectedBgColor(ConsoleColor color)
        {
            SelBgColor = color;
            return this;
        }
        public InteractiveMenu SetSelectedFgColor(ConsoleColor color)
        {
            SelFgColor = color;
            return this;
        }

        public InteractiveMenu SetCaption(string caption)
        {
            Caption = caption;
            return this;
        }
        public InteractiveMenu AddOption(MenuOption option)
        {
            if (Options.Count >= 9)
            {
                throw new InvalidOperationException("Max option limit reached");
            }

            Options.Add(option);
            return this;
        }
        public bool Remove(MenuOption option)
        {
            return Options.Remove(option);
        }

        /// <summary>
        /// Shows menu and return selected option index
        /// </summary>
        /// <returns>Selected option index</returns>
        public int ShowMenu()
        {
            var orgBgColor = Console.BackgroundColor;
            var orgFgColor = Console.ForegroundColor;

            int val = executeMenu();

            Console.BackgroundColor = orgBgColor;
            Console.ForegroundColor = orgFgColor;

            return val;
        }

        private int executeMenu()
        {
            int selOption = 0;
            bool skipDraw = false;
            while (true)
            {
                if (!skipDraw)
                {
                    Console.Clear();
                    if (!string.IsNullOrEmpty(Caption)) Console.WriteLine(Caption);

                    for (int i = 0; i < Options.Count; i++)
                    {
                        // Set selected color
                        if (i == selOption)
                        {
                            Console.BackgroundColor = SelBgColor;
                            Console.ForegroundColor = SelFgColor;
                        }

                        if (ShowOptionNumbers)
                        {
                            Console.Write($"{i + 1}. ");
                        }
                        if (ShowOptionHotKeys && Options[i].HotKey.HasValue)
                        {
                            Console.Write($"[{Options[i].HotKey}] ");
                        }

                        Console.WriteLine($"{Options[i].Text}");

                        // Reset selected color
                        if (i == selOption)
                        {
                            Console.BackgroundColor = ClearBgColor;
                            Console.ForegroundColor = ClearFgColor;
                        }
                    }
                }

                skipDraw = false;
                var key = Console.ReadKey(true);
                // Process seletion keys
                if (key.Key == ConsoleKey.DownArrow)
                {
                    if (selOption < Options.Count - 1) selOption++;
                    continue;
                }
                if (key.Key == ConsoleKey.UpArrow)
                {
                    if (selOption > 0) selOption--;
                    continue;
                }
                if (key.Key == ConsoleKey.Enter) return selOption;

                // process HotKeys
                for (int i = 0; i < Options.Count; i++)
                {
                    if (Options[i] == null) continue;
                    if (Options[i].HotKey == null) continue;

                    if (Options[i].HotKey.Value == key.Key) return i;
                }
                // None, just ignore and do not redraw
                skipDraw = true;
            }
        }

        public class MenuOption
        {
            public string Text { get; set; } = string.Empty;
            public ConsoleKey? HotKey { get; set; }
        }

    }
}
#endif
