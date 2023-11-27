namespace PandaDotNet.Cache.Abstraction
{
    /// <summary>
    /// Describes a factory class that returns new cache instances
    /// </summary>
    public interface ICacheFactory
    {
        /// <summary>
        /// Returns a new cache instance for the given key-value combination
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <returns></returns>
        ICache<TObject, TKey> GetNewCache<TObject, TKey>();
    }
}