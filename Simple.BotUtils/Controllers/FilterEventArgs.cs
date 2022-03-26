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

        public T GetArg<T>()
        {
            if (Args == null) return default;
            
            if(GetArg(out T t)) return t;
            return default;
        }
        public bool GetArg<T>(out T t)
        {
            t = default;
            if (Args == null) return false;

            for (int i = 0; i < Args.Length; i++)
            {
                if (Args[i] == null) continue;
                if(Args[i].GetType() == typeof(T))
                {
                    t = (T)Args[i];
                    return true;
                }
            }
            return false;
        }

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
