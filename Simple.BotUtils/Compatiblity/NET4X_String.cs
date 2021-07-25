#if NET40_OR_GREATER || NETSTANDARD
namespace Simple.BotUtils
{
    internal static class NET4X_String
    {
        public static bool EndsWith(this string text, char value)
        {
            int thisLen = text.Length;
            if (thisLen != 0)
            {
                if (text[thisLen - 1] == value)
                    return true;
            }
            return false;
        }
        public static bool StartsWith(this string text, char value)
        {
            int thisLen = text.Length;
            if (thisLen != 0)
            {
                if (text[0] == value)
                    return true;
            }
            return false;
        }
    }
}
#endif