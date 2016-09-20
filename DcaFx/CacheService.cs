using System;
using System.Runtime.Caching;

namespace DcaFx
{
    
    public class InMemoryCache : ICacheService
    {
        public T GetOrSet<T>(string cacheKey, double expiresIn, Func<T> getItemCallback) where T : class
        {
            T item = MemoryCache.Default.Get(cacheKey) as T;
            if (item == null)
            {
                item = getItemCallback();
                if(item != null) MemoryCache.Default.Add(cacheKey, item, DateTime.Now.AddSeconds(expiresIn));
            }
            return item;
        }
    }

    interface ICacheService
    {
        T GetOrSet<T>(string cacheKey, double expiresIn, Func<T> getItemCallback) where T : class;
    }
}