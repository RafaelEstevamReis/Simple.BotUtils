using System.Collections.Generic;
using System.Collections.Specialized;

namespace Simple.BotUtils.Startup
{
    public class Arguments : Dictionary<string, string>
    {
        public bool Has(string key) => ContainsKey(key);

        public string Get(string key)
        {
            if (TryGetValue(key, out string val)) return val;
            return null;
        }

#if !NETSTANDARD1_0
        public NameValueCollection ToNameValue()
        {
            NameValueCollection nvc = new NameValueCollection();
            foreach (var pair in this)
            {
                nvc[pair.Key] = pair.Value;
            }
            return nvc;
        }
#endif

    }
}
