#if !NETSTANDARD1_0

using System;
using System.Collections.Generic;
using System.Text;

namespace Simple.BotUtils.Terminal
{
    /// <summary>
    /// Interactive, arrow-driven console menu. Navigate with the arrow keys and pick with Enter.
    /// Options can also be triggered by a hotkey: the first 9 options registered without an explicit
    /// <see cref="MenuOption.HotKey"/> automatically receive the number keys 1..9 (skipping any digit
    /// already used explicitly), so numbers and hotkeys are a single mechanic.
    /// </summary>
    public class InteractiveMenu
    {
        /// <summary>Value returned by <see cref="ShowMenu"/> when the menu is canceled</summary>
        public const int Canceled = -1;

        /// <summary>Background color of non-selected rows and captions</summary>
        public ConsoleColor ClearBgColor { get; set; } = ConsoleColor.Black;
        /// <summary>Foreground color of non-selected rows and captions</summary>
        public ConsoleColor ClearFgColor { get; set; } = ConsoleColor.White;
        /// <summary>Background color of the currently selected row</summary>
        public ConsoleColor SelBgColor { get; set; } = ConsoleColor.White;
        /// <summary>Foreground color of the currently selected row</summary>
        public ConsoleColor SelFgColor { get; set; } = ConsoleColor.Black;
        /// <summary>Foreground color used for disabled options</summary>
        public ConsoleColor DisabledFgColor { get; set; } = ConsoleColor.DarkGray;
        /// <summary>Foreground color used for the hints line and scroll indicators</summary>
        public ConsoleColor HintFgColor { get; set; } = ConsoleColor.DarkGray;

        /// <summary>Show each option's hotkey (e.g. <c>[1]</c>, <c>[F4]</c>) before its text</summary>
        public bool ShowOptionHotKeys { get; set; } = true;
        /// <summary>Draw the key-bindings hints line below the options</summary>
        public bool ShowHints { get; set; } = true;
        /// <summary>Clear the console before each redraw</summary>
        public bool ClearScreen { get; set; } = true;
        /// <summary>Wrap around when navigating past the first/last option</summary>
        public bool WrapNavigation { get; set; } = true;
        /// <summary>Allow leaving the menu with <see cref="CancelKey"/>, returning <see cref="Canceled"/></summary>
        public bool AllowCancel { get; set; } = true;
        /// <summary>Key that cancels the menu when <see cref="AllowCancel"/> is enabled</summary>
        public ConsoleKey CancelKey { get; set; } = ConsoleKey.Escape;
        /// <summary>Marker drawn before the currently selected option</summary>
        public string SelectionIndicator { get; set; } = "> ";
        /// <summary>Max options drawn at once (0 = auto fit to the console window). Longer lists scroll</summary>
        public int MaxVisibleOptions { get; set; } = 0;

        /// <summary>Optional title drawn above the options</summary>
        public string Caption { get; set; } = string.Empty;
        /// <summary>The menu options, in registration order</summary>
        public List<MenuOption> Options { get; private set; } = [];

        // Effective hotkey per option index, resolved while the menu is being shown
        private ConsoleKey?[] hotKeys = [];

        /// <summary>Sets <see cref="ClearBgColor"/> and returns this menu for chaining</summary>
        public InteractiveMenu SetClearBgColor(ConsoleColor color)
        {
            ClearBgColor = color;
            return this;
        }
        /// <summary>Sets <see cref="ClearFgColor"/> and returns this menu for chaining</summary>
        public InteractiveMenu SetClearFgColor(ConsoleColor color)
        {
            ClearFgColor = color;
            return this;
        }
        /// <summary>Sets <see cref="SelBgColor"/> and returns this menu for chaining</summary>
        public InteractiveMenu SetSelectedBgColor(ConsoleColor color)
        {
            SelBgColor = color;
            return this;
        }
        /// <summary>Sets <see cref="SelFgColor"/> and returns this menu for chaining</summary>
        public InteractiveMenu SetSelectedFgColor(ConsoleColor color)
        {
            SelFgColor = color;
            return this;
        }
        /// <summary>Sets <see cref="Caption"/> and returns this menu for chaining</summary>
        public InteractiveMenu SetCaption(string caption)
        {
            Caption = caption;
            return this;
        }
        /// <summary>Sets <see cref="MaxVisibleOptions"/> and returns this menu for chaining</summary>
        public InteractiveMenu SetMaxVisibleOptions(int count)
        {
            MaxVisibleOptions = count;
            return this;
        }

        /// <summary>Adds an option and returns this menu for chaining</summary>
        public InteractiveMenu AddOption(MenuOption option)
        {
            Options.Add(option);
            return this;
        }
        /// <summary>Adds an option with the given text and returns this menu for chaining</summary>
        public InteractiveMenu AddOption(string text)
            => AddOption(new MenuOption { Text = text });
        /// <summary>Adds an option with an explicit hotkey and returns this menu for chaining</summary>
        public InteractiveMenu AddOption(string text, ConsoleKey hotKey)
            => AddOption(new MenuOption { Text = text, HotKey = hotKey });
        /// <summary>Adds an option with an action (see <see cref="Run"/>) and returns this menu for chaining</summary>
        public InteractiveMenu AddOption(string text, Action action)
            => AddOption(new MenuOption { Text = text, Action = action });
        /// <summary>Adds an option with an action (see <see cref="Run"/>) and returns this menu for chaining</summary>
        public InteractiveMenu AddOption<T>(string text, Action<T> action, T args)
            => AddOption(new MenuOption { Text = text, Action = () => action(args) });
        /// <summary>Adds an option with an explicit hotkey and action and returns this menu for chaining</summary>
        public InteractiveMenu AddOption(string text, ConsoleKey hotKey, Action action)
            => AddOption(new MenuOption { Text = text, HotKey = hotKey, Action = action });
        /// <summary>Adds an option with an explicit hotkey and action and returns this menu for chaining</summary>
        public InteractiveMenu AddOption<T>(string text, ConsoleKey hotKey, Action<T> action, T args)
            => AddOption(new MenuOption { Text = text, HotKey = hotKey, Action = () => action(args) });
        /// <summary>Removes an option. Returns true if it was present</summary>
        public bool Remove(MenuOption option)
            => Options.Remove(option);

        /// <summary>
        /// Shows the menu and returns the selected <see cref="MenuOption"/>, or null if canceled
        /// </summary>
        public MenuOption? Show()
        {
            if (Options.Count == 0) throw new InvalidOperationException("Menu has no options");

            var orgBgColor = Console.BackgroundColor;
            var orgFgColor = Console.ForegroundColor;
            hotKeys = resolveHotKeys();
            try
            {
                int index = executeMenu();
                return index < 0 ? null : Options[index];
            }
            finally
            {
                Console.BackgroundColor = orgBgColor;
                Console.ForegroundColor = orgFgColor;
                hotKeys = [];
            }
        }
        /// <summary>
        /// Shows the menu, invokes the selected option's <see cref="MenuOption.Action"/> (if any)
        /// and returns the chosen option, or null if canceled
        /// </summary>
        public MenuOption? Run()
        {
            var option = Show();
            option?.Action?.Invoke();
            return option;
        }

        // Resolves the effective hotkey of every option: explicit keys are kept, then the first 9
        // options without one receive the digits 1..9 that aren't already claimed explicitly.
        private ConsoleKey?[] resolveHotKeys()
        {
            var keys = new ConsoleKey?[Options.Count];
            var used = new HashSet<ConsoleKey>();

            for (int i = 0; i < Options.Count; i++)
            {
                var option = Options[i];
                if (option?.HotKey != null)
                {
                    keys[i] = option.HotKey;
                    used.Add(option.HotKey.Value);
                }
            }

            int nextDigit = 0;
            for (int i = 0; i < Options.Count; i++)
            {
                if (Options[i] == null || keys[i] != null) continue;
                while (nextDigit < 9)
                {
                    var candidate = (ConsoleKey)((int)ConsoleKey.D1 + nextDigit);
                    nextDigit++;
                    if (used.Contains(candidate)) continue;
                    keys[i] = candidate;
                    used.Add(candidate);
                    break;
                }
            }
            return keys;
        }

        private int executeMenu()
        {
            int selOption = firstSelectable(0, +1);
            int scrollOffset = 0;
            bool redraw = true;

            while (true)
            {
                if (redraw) draw(selOption, ref scrollOffset);
                redraw = true;

                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.DownArrow: selOption = move(selOption, +1); continue;
                    case ConsoleKey.UpArrow: selOption = move(selOption, -1); continue;
                    case ConsoleKey.Home: selOption = firstSelectable(0, +1); continue;
                    case ConsoleKey.End: selOption = firstSelectable(Options.Count - 1, -1); continue;
                    case ConsoleKey.PageDown: selOption = page(selOption, +1); continue;
                    case ConsoleKey.PageUp: selOption = page(selOption, -1); continue;
                    case ConsoleKey.Enter:
                        if (isSelectable(selOption)) return selOption;
                        redraw = false;
                        continue;
                }

                // HotKeys (including the auto-assigned 1..9) take precedence over the cancel key
                int hotKey = findHotKey(normalizeKey(key.Key));
                if (hotKey >= 0) return hotKey;

                if (AllowCancel && key.Key == CancelKey) return Canceled;

                // Unknown key: ignore without redrawing to avoid flicker
                redraw = false;
            }
        }

        private bool isSelectable(int index)
            => index >= 0 && index < Options.Count && Options[index] != null && Options[index].Enabled;

        private int firstSelectable(int start, int dir)
        {
            for (int i = start; i >= 0 && i < Options.Count; i += dir)
            {
                if (isSelectable(i)) return i;
            }
            return -1;
        }

        private int move(int current, int dir)
        {
            int count = Options.Count;
            int next = current;
            for (int step = 0; step < count; step++)
            {
                next += dir;
                if (next < 0)
                {
                    if (!WrapNavigation) return current;
                    next = count - 1;
                }
                else if (next >= count)
                {
                    if (!WrapNavigation) return current;
                    next = 0;
                }
                if (isSelectable(next)) return next;
            }
            return current;
        }

        private int page(int current, int dir)
        {
            int visible = visibleCount();
            int target = current + (dir * visible);
            if (target < 0) target = 0;
            if (target >= Options.Count) target = Options.Count - 1;

            int found = nearestSelectable(target, dir);
            if (found < 0) found = nearestSelectable(target, -dir);
            return found < 0 ? current : found;
        }
        private int nearestSelectable(int start, int dir)
        {
            for (int i = start; i >= 0 && i < Options.Count; i += dir)
            {
                if (isSelectable(i)) return i;
            }
            return -1;
        }

        private int findHotKey(ConsoleKey key)
        {
            for (int i = 0; i < Options.Count; i++)
            {
                if (!isSelectable(i)) continue;
                if (hotKeys[i] == key) return i;
            }
            return -1;
        }

        // Treats the numeric keypad digits as their top-row equivalents
        private static ConsoleKey normalizeKey(ConsoleKey key)
        {
            if (key >= ConsoleKey.NumPad0 && key <= ConsoleKey.NumPad9)
            {
                return (ConsoleKey)((int)ConsoleKey.D0 + (key - ConsoleKey.NumPad0));
            }
            return key;
        }

        private int visibleCount()
        {
            if (MaxVisibleOptions > 0) return Math.Min(MaxVisibleOptions, Options.Count);

            try
            {
                int reserved = 2; // scroll indicators
                if (!string.IsNullOrEmpty(Caption)) reserved += captionLineCount();
                if (ShowHints) reserved += 2; // blank line + hints
                int height = Console.WindowHeight - reserved;
                if (height < 1) height = 1;
                return Math.Min(height, Options.Count);
            }
            catch
            {
                // Console size is unavailable (e.g. redirected output)
                return Options.Count;
            }
        }
        private int captionLineCount()
        {
            int lines = 1;
            foreach (var c in Caption)
            {
                if (c == '\n') lines++;
            }
            return lines;
        }

        private void draw(int selOption, ref int scrollOffset)
        {
            if (ClearScreen) Console.Clear();

            int count = Options.Count;
            int visible = visibleCount();
            bool scrollable = count > visible;

            // Keep the selected option inside the viewport
            if (selOption >= 0)
            {
                if (selOption < scrollOffset) scrollOffset = selOption;
                else if (selOption >= scrollOffset + visible) scrollOffset = selOption - visible + 1;
            }
            int maxOffset = Math.Max(0, count - visible);
            if (scrollOffset > maxOffset) scrollOffset = maxOffset;
            if (scrollOffset < 0) scrollOffset = 0;

            resetColors();
            if (!string.IsNullOrEmpty(Caption)) Console.WriteLine(Caption);

            if (scrollable && scrollOffset > 0) writeHint("  ^ ...");

            int end = Math.Min(count, scrollOffset + visible);
            for (int i = scrollOffset; i < end; i++)
            {
                drawOption(i, i == selOption);
            }

            if (scrollable && end < count) writeHint("  v ...");

            if (ShowHints) drawHints(scrollable);

            resetColors();
        }

        private void drawOption(int index, bool selected)
        {
            var option = Options[index];
            bool enabled = option != null && option.Enabled;
            bool highlight = selected && enabled;

            Console.BackgroundColor = highlight ? SelBgColor : ClearBgColor;
            Console.ForegroundColor = highlight ? SelFgColor : (enabled ? ClearFgColor : DisabledFgColor);

            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(SelectionIndicator))
            {
                sb.Append(highlight ? SelectionIndicator : new string(' ', SelectionIndicator.Length));
            }

            var hotKey = hotKeys[index];
            if (ShowOptionHotKeys && hotKey != null) sb.Append('[').Append(hotKeyLabel(hotKey.Value)).Append("] ");

            sb.Append(option?.Text);

            var description = option?.Description;
            if (!string.IsNullOrEmpty(description)) sb.Append("  - ").Append(description);

            Console.WriteLine(sb.ToString());
        }

        private void drawHints(bool scrollable)
        {
            var sb = new StringBuilder();
            sb.Append("[Up/Down] navigate  [Enter] select");
            if (scrollable) sb.Append("  [PgUp/PgDn/Home/End] scroll");
            if (ShowOptionHotKeys) sb.Append("  [key] jump");
            if (AllowCancel) sb.Append("  [").Append(CancelKey).Append("] cancel");

            Console.WriteLine();
            Console.BackgroundColor = ClearBgColor;
            Console.ForegroundColor = HintFgColor;
            Console.WriteLine(sb.ToString());
        }

        // Renders digit keys as "1".."9" and everything else by its enum name (e.g. "F4")
        private static string hotKeyLabel(ConsoleKey key)
        {
            if (key >= ConsoleKey.D0 && key <= ConsoleKey.D9)
            {
                return ((char)('0' + (key - ConsoleKey.D0))).ToString();
            }
            return key.ToString();
        }

        private void writeHint(string text)
        {
            Console.BackgroundColor = ClearBgColor;
            Console.ForegroundColor = HintFgColor;
            Console.WriteLine(text);
            resetColors();
        }
        private void resetColors()
        {
            Console.BackgroundColor = ClearBgColor;
            Console.ForegroundColor = ClearFgColor;
        }

        /// <summary>A single menu entry</summary>
        public class MenuOption
        {
            /// <summary>Text shown for this option</summary>
            public string Text { get; set; } = string.Empty;
            /// <summary>Optional secondary text drawn dimmed after <see cref="Text"/></summary>
            public string? Description { get; set; }
            /// <summary>Explicit hotkey; when null the option may auto-receive a digit 1..9</summary>
            public ConsoleKey? HotKey { get; set; }
            /// <summary>Whether the option can be selected. Disabled options are skipped and dimmed</summary>
            public bool Enabled { get; set; } = true;
            /// <summary>Action invoked by <see cref="InteractiveMenu.Run"/> when this option is chosen</summary>
            public Action? Action { get; set; }
            /// <summary>Arbitrary user data associated with this option</summary>
            public object? Tag { get; set; }
        }

    }
}
#endif
