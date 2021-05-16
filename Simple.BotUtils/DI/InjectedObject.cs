using System;

namespace Simple.BotUtils.DI
{
    internal class InjectedObject
    {
        public Func<object> Constructor { get; set; }
        public object Instance { get; set; }
        public InjectionType InjectionType { get; set; }
    }
}
