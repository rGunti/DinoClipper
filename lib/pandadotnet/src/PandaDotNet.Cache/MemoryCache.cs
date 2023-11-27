using System.Collections.Concurrent;
using System.Threading.Tasks;
using PandaDotNet.Cache.Abstraction;

namespace PandaDotNet.Cache
{
    /// <summary>
    /// A simple, <see cref="ConcurrentDictionary{TKey,TValue}"/>-based implementation of the
    /// <see cref="ICache{TObject,TKey}"/> interface.
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class MemoryCache<TObject, TKey> : ICache<TObject, TKey>
    {
        /// <summary>
        /// The cache storing the objects itself.
        /// </summary>
        protected readonly ConcurrentDictionary<TKey, TObject> _objects = new();

        /// <inheritdoc />
        public virtual void CacheObject(TObject obj, TKey key)
        {
            _objects.AddOrUpdate(key, _ => obj, (_, _) => obj);
        }

        /// <inheritdoc />
        public virtual bool IsCached(TKey key)
        {
            return _objects.ContainsKey(key);
        }

        /// <inheritdoc />
        public virtual TObject GetObjectForKey(TKey key, ValueFactoryDelegate<TObject, TKey> valueFactory = null)
        {
            if (valueFactory != null)
            {
                return _objects.GetOrAdd(key, k => valueFactory(k));
            }

            if (_objects.TryGetValue(key, out TObject obj))
            {
                return obj;
            }

            return default;
        }

        /// <inheritdoc />
        public virtual async Task<TObject> GetObjectForKeyAsync(
            TKey key,
            AsyncValueFactoryDelegate<TObject, TKey> valueFactory = null)
        {
            if (_objects.TryGetValue(key, out TObject obj))
            {
                return obj;
            }

            if (valueFactory != null)
            {
                obj = await valueFactory(key);
                CacheObject(obj, key);
                return obj;
            }

            return default;
        }

        /// <inheritdoc />
        public virtual void RemoveFromCache(TKey key)
        {
            _objects.TryRemove(key, out TObject _);
        }

        /// <inheritdoc />
        public virtual void ClearCache()
        {
            _objects.Clear();
        }

        /// <inheritdoc />
        public virtual CacheMetrics GetMetrics()
        {
            return new CacheMetrics
            {
                CacheType = GetType(),
                CachedObjects = _objects.Count
            };
        }
    }
}