using System;

namespace PandaDotNet.Cache.ExpiringCache
{
    /// <summary>
    /// Wraps a cached object and adds metadata to it.
    /// </summary>
    /// <typeparam name="TObject">The object to be cached</typeparam>
    public class CacheItem<TObject>
    {
        /// <summary>
        /// The timestamp when <see cref="Object"/> was retrieved and cached
        /// </summary>
        public DateTime CachedAt { get; set; }
        
        /// <summary>
        /// The cached object
        /// </summary>
        public TObject Object { get; set; }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Cached {typeof(TObject)} @ {CachedAt}: {Object}";
        }
    }
}