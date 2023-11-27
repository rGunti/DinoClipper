using System;
using System.Linq;
using System.Threading.Tasks;
using PandaDotNet.Cache.Abstraction;
using PandaDotNet.Time;

namespace PandaDotNet.Cache.ExpiringCache
{
    /// <summary>
    /// An extension of <see cref="MemoryCache{TObject,TKey}"/>
    /// which expires objects after a given timestamp.
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class ExpiringMemoryCache<TObject, TKey> :
        MemoryCache<CacheItem<TObject>, TKey>,
        ICache<TObject, TKey>
    {
        private readonly TimeSpan _maxCacheAge;
        private readonly IClock _clock;

        /// <summary>
        /// Initializes a new Cache instance.
        /// </summary>
        /// <param name="maxCacheAge">The maximum timespan an object may reside in cache.</param>
        /// <param name="clock">An implementation of <see cref="IClock"/> to get the current time from</param>
        public ExpiringMemoryCache(TimeSpan maxCacheAge, IClock clock)
        {
            _maxCacheAge = maxCacheAge;
            _clock = clock;
        }

        private CacheItem<TObject> ConstructNewCacheItem(TObject obj)
        {
            return new CacheItem<TObject>
            {
                Object = obj,
                CachedAt = _clock.GetCurrentDateTimeUtc()
            };
        }

        /// <inheritdoc />
        public virtual void CacheObject(TObject obj, TKey key)
        {
            CacheObject(ConstructNewCacheItem(obj), key);
        }

        private bool IsExpired(CacheItem<TObject> item)
        {
            return _clock.GetCurrentDateTimeUtc() - item.CachedAt > _maxCacheAge;
        }

        /// <summary>
        /// Checks if an item exists in cache and if it hasn't already expired.
        /// Will therefore also return false, if the object is available in cache
        /// but was cached for longer than the maximum age.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override bool IsCached(TKey key)
        {
            if (_objects.TryGetValue(key, out CacheItem<TObject> item))
            {
                return !IsExpired(item);
            }
            return false;
        }

        /// <inheritdoc />
        public virtual TObject GetObjectForKey(TKey key, ValueFactoryDelegate<TObject, TKey> valueFactory = null)
        {
            if (_objects.TryGetValue(key, out CacheItem<TObject> item))
            {
                if (!IsExpired(item))
                {
                    return item.Object;
                }
            }

            if (valueFactory != null)
            {
                TObject obj = valueFactory(key);
                CacheObject(obj, key);
                return obj;
            }

            return default;
        }

        /// <inheritdoc />
        public virtual async Task<TObject> GetObjectForKeyAsync(
            TKey key,
            AsyncValueFactoryDelegate<TObject, TKey> valueFactory = null)
        {
            if (_objects.TryGetValue(key, out CacheItem<TObject> item))
            {
                if (!IsExpired(item))
                {
                    return item.Object;
                }
            }

            if (valueFactory != null)
            {
                TObject obj = await valueFactory(key);
                CacheObject(obj, key);
                return obj;
            }

            return default;
        }

        /// <inheritdoc />
        public override CacheMetrics GetMetrics()
        {
            return new ExpiringMemoryCacheMetrics
            {
                CacheType = GetType(),
                CachedObjects = _objects.Count,
                ExpiredItems = _objects.Values.Count(IsExpired)
            };
        }
    }
}