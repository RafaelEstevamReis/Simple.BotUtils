using Simple.BotUtils.Caching;
using System;

namespace Simple.BotUtils.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var cache = new MemoryCache();
            cache.Add("USERS", new CacheOptions()
            {
                ExpirationPolicy = ExpirationPolicy.LastAccess,
                ExpirationValue = TimeSpan.FromHours(1),
                UpdateCallback = () => new long[] { 12345566 },
            });

            var data = cache.Get<long[]>("USERS");
            data = data;
        }
    }
}
