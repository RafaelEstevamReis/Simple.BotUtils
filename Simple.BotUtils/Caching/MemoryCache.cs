using System;
using System.Collections.Generic;

namespace Simple.BotUtils.Caching
{
    public class MemoryCache
    {
        Dictionary<string, CacheItem> items;
        DateTime lastMaintenance;

        public MemoryCache()
        {
            items = new Dictionary<string, CacheItem>();
            lastMaintenance = DateTime.Now;
        }

        public void Add(string key, CacheOptions options)
        {
            items[key] = new CacheItem(options);
        }

        public bool TryGet(string key, out object value)
        {
            doMaintenance();

            
            CacheItem item = GetItemInfo(key);
            
            item.TryRenew();

            value = null;
            if (item.Expired) return false;

            value = item.Retrieve();
            return true;
        }
        public bool TryGet<T>(string key, out T value)
        {
            value = default;
            if (!TryGet(key, out object obj)) return false;

            value = (T)obj;
            return true;
        }

        public T Get<T>(string key)
        {
            if (!TryGet<T>(key, out T value)) throw new Exception("Cache expired");
            return value;
        }

        public void Invalidate(string key)
        {
            GetItemInfo(key).Invalidate();
        }

        public CacheItem GetItemInfo(string key)
        {
            if (!items.ContainsKey(key)) throw new KeyNotFoundException();

            return items[key];
        }

        private void doMaintenance()
        {
            if ((DateTime.Now - lastMaintenance).TotalMinutes < 1) return;

            lock (items)
            {
                foreach (var value in items.Values) value.FreeExpired();
                lastMaintenance = DateTime.Now;
            }
        }

    }
}
