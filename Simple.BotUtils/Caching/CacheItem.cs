using System;

namespace Simple.BotUtils.Caching
{
    public class CacheItem
    {
        public CacheOptions CreationOptions { get; private set; }
        public DateTime AddedOn { get; private set; }
        public DateTime? LastAccess { get; private set; }
        public DateTime? LastUpdate { get; private set; }
      
        public DateTime ExpiresOn
        {
            get
            {
                if (CreationOptions.ExpirationPolicy == ExpirationPolicy.LastUpdate) return (LastUpdate ?? AddedOn) + CreationOptions.ExpirationValue;
                if (CreationOptions.ExpirationPolicy == ExpirationPolicy.LastAccess)
                {
                    return (LastAccess ?? LastUpdate ?? AddedOn) + CreationOptions.ExpirationValue;
                }
                return AddedOn + CreationOptions.ExpirationValue;
            }
        }

        public bool IsValid
        {
            get
            {
                if (!canBeUsed) return false;
                if (Expired) return false;
                return true;
            }
        }

        private object currentValue; 
        private bool canBeUsed;


        public CacheItem(CacheOptions options)
        {
            CreationOptions = options;
            AddedOn = DateTime.Now;
            LastAccess = null;
            LastUpdate = null;
            canBeUsed = false;
        }

        public bool Expired => ExpiresOn > DateTime.Now;

        public object Retrieve()
        {
            LastAccess = DateTime.Now;
            return currentValue;
        }

        public void TryRenew()
        {
            if (IsValid) return;

            if (CreationOptions.ExpirationPolicy == ExpirationPolicy.DoNotRenew) return;

            // Update !
            update();
        }

        public void FreeExpired()
        {
            if (Expired) currentValue = null;
        }

        public void Invalidate()
        {
            canBeUsed = false;
            currentValue = null;
        }

        private void update()
        {
            if (CreationOptions.UpdateCallback == null) return;

            currentValue = CreationOptions.UpdateCallback;
            LastUpdate = DateTime.Now;
            canBeUsed = true;
        }

    }
}
