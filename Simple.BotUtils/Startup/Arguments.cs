using System.Collections.Generic;

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
    }
}
