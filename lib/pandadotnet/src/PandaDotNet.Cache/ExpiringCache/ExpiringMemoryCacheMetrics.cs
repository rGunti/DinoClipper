using PandaDotNet.Cache.Abstraction;

namespace PandaDotNet.Cache.ExpiringCache
{
    /// <summary>
    /// An extension of <see cref="CacheMetrics"/> for <see cref="ExpiringMemoryCache{TObject,TKey}"/>.
    /// </summary>
    public class ExpiringMemoryCacheMetrics : CacheMetrics
    {
        /// <summary>
        /// Returns the number of expired items in the cache
        /// </summary>
        public int ExpiredItems { get; set; }
    }
}