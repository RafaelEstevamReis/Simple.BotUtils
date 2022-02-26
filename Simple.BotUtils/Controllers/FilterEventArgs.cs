using System;

namespace Simple.BotUtils.Controllers
{
    public class FilterEventArgs : EventArgs
    {
        public string Method { get; set; }
        public object[] Args { get; set; }
        public Attribute[] Attrbiutes { get;  set; }
        public FilterException BlockReason { get; set; }
    }
}
