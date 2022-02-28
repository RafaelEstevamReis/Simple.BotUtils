#if !NETSTANDARD1_0

using System;
using System.Collections.Generic;

namespace Simple.BotUtils.Terminal
{
    public class SimpleMenu
    {
        public string Caption { get; set; }
        public List<string> Options { get; private set; }
        public SimpleMenu()
        {
            Options = new List<string>();
        }

        public SimpleMenu SetCaption(string caption)
        {
            Caption = caption;
            return this;
        }

        public SimpleMenu AddOption(string option)
        {
            if (Options.Count >= 9)
            {
                throw new InvalidOperationException("Max option limit reached");
            }

            Options.Add(option);
            return this;
        }
        public bool Remove(string option)
        {
            return Options.Remove(option);
        }

        public int ShowMenu(bool cleanScreen)
        {
            if (cleanScreen) Console.Clear();
            if (!string.IsNullOrEmpty(Caption)) Console.WriteLine(Caption);

            for (int i = 0; i < Options.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {Options[i]}");
            }

            Console.Write(">_ ");
            string opt = Console.ReadLine();

            if(!int.TryParse(opt, out int value))
            {
                return ShowMenu(cleanScreen);
            }

            if(value > Options.Count)
            {
                return ShowMenu(cleanScreen);
            }

            return value - 1;
        }
    }
}
#endif
