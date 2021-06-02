using System;

namespace Simple.BotUtils.Startup
{
    public class ArgumentKeyAttribute : Attribute
    {
        public string[] Keys { get; }
        public ArgumentKeyAttribute(params string[] keys)
        {
            Keys = keys;
        }
    }
}
