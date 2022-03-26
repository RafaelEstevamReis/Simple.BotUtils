using System;
using System.Linq;

namespace Simple.BotUtils.Controllers
{
    public class FilterEventArgs : EventArgs
    {
        public string Method { get; set; }
        public object[] Args { get; set; }
        public Attribute[] Attrbiutes { get;  set; }
        public FilterException BlockReason { get; set; }

        public T GetAttribute<T>()
            where T : Attribute
        {
            if (Attrbiutes == null) return null;
            
            return Attrbiutes.OfType<T>().FirstOrDefault();            
        }
        public bool HasAttribute<T>()
            where T : Attribute
        {
            return GetAttribute<T>() != null;
        }
    }
}
