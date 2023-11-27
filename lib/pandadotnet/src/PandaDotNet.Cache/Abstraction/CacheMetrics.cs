using System;

namespace PandaDotNet.Cache.Abstraction
{
    /// <summary>
    /// Describes the status of a cache instance.
    /// This class may be extended as required.
    /// </summary>
    public class CacheMetrics
    {
        /// <summary>
        /// Returns the type of the cache these metrics came from.
        /// </summary>
        public Type CacheType { get; set; }
        
        /// <summary>
        /// Returns the number of objects stored in a cache
        /// </summary>
        public int CachedObjects { get; set; }
    }
}