using System.Threading.Tasks;

namespace PandaDotNet.Cache.Abstraction
{
    /// <summary>
    /// Describes a function that retrieves and object from a data source using the
    /// provided key for caching. 
    /// </summary>
    /// <typeparam name="TObject">The object to be cached</typeparam>
    /// <typeparam name="TKey">The key used to access a cached object</typeparam>
    public delegate TObject ValueFactoryDelegate<out TObject, in TKey>(TKey key);
    
    /// <summary>
    /// An asynchronous version of <see cref="ValueFactoryDelegate{TObject,TKey}"/>.
    /// </summary>
    /// <typeparam name="TObject">The object to be cached</typeparam>
    /// <typeparam name="TKey">The key used to access a cached object</typeparam>
    public delegate Task<TObject> AsyncValueFactoryDelegate<TObject, in TKey>(TKey key);
    
    /// <summary>
    /// A basic interface for caching object using a primary key.
    /// </summary>
    /// <typeparam name="TObject">The object to be cached</typeparam>
    /// <typeparam name="TKey">The key used to access a cached object</typeparam>
    public interface ICache<TObject, TKey>
    {
        /// <summary>
        /// Stores an object in cache
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="key"></param>
        void CacheObject(TObject obj, TKey key);

        /// <summary>
        /// Checks if an object with the given key resides in cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool IsCached(TKey key);

        /// <summary>
        /// Retrieves an object from the cache using the provided key. If the object does not reside in the cache,
        /// it will be retrieved from the data source, if one is provided using the <see cref="valueFactory"/>
        /// parameter.
        /// </summary>
        /// <param name="key">The key of the object to retrieve</param>
        /// <param name="valueFactory">An optional method to get the desired object from a data source</param>
        /// <returns></returns>
        TObject GetObjectForKey(TKey key, ValueFactoryDelegate<TObject, TKey> valueFactory = null);

        /// <summary>
        /// Retrieves an object from the cache using the provided key. If the object does not reside in the cache,
        /// it will be retrieved from the data source, if one is provided using the <see cref="valueFactory"/>
        /// parameter. This method supports async calls.
        /// </summary>
        /// <param name="key">The key of the object to retrieve</param>
        /// <param name="valueFactory">An optional method to get the desired object from a data source</param>
        /// <returns></returns>
        Task<TObject> GetObjectForKeyAsync(TKey key, AsyncValueFactoryDelegate<TObject, TKey> valueFactory = null);

        /// <summary>
        /// Removes an item with the given key from cache, if it exists.
        /// </summary>
        /// <param name="key"></param>
        void RemoveFromCache(TKey key);

        /// <summary>
        /// Forces the cache to clear.
        /// </summary>
        void ClearCache();

        /// <summary>
        /// Returns a new <see cref="CacheMetrics"/> object describing the current state of this cache.
        /// </summary>
        /// <returns></returns>
        CacheMetrics GetMetrics();
    }
}