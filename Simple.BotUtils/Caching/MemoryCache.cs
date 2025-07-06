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

        public bool HasKey(string key) => items.ContainsKey(key);

        public void Add(string key, CacheOptions options)
        {
            items[key] = new CacheItem(options);
        }

        public bool TryGet(string key, out object value)
        {
            doMaintenance();

            if (!items.ContainsKey(key))
            {
                value = null;
                return false;
            }
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
        public object Get(string key)
        {
            if (!TryGet(key, out object value)) throw new Exception("Cache expired");
            return value;
        }

        public object GetOrAdd(string key, CacheOptions options)
        {
            if (!items.ContainsKey(key)) Add(key, options);

            return Get(key);
        }
        public T GetOrAdd<T>(string key, CacheOptions options)
        {
            if (!items.ContainsKey(key)) Add(key, options);

            return Get<T>(key);
        }
        /// <summary>
        /// Invalidates an entry
        /// </summary>
        /// <param name="key">Entry key to be invalidated</param>
        /// <returns>True if the value was invalidated, False if the key does not exists</returns>
        public bool Invalidate(string key)
        {
            if (!items.ContainsKey(key)) return false;

            GetItemInfo(key).Invalidate();
            return true;
        }

        public void UpdateValue<T>(string key, T value)
        {
            var info = GetItemInfo(key);
            info.UpdateValue(value);
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
