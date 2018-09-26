using System.Runtime.Caching;

namespace Agoda.Analyzers.Test.Helpers.GenericTestHelpers
{
    /// <summary>
    /// Provides in memory caching mechanism
    /// </summary>
    public static class CacheManager
    {
        /// <summary>
        /// Returns the value from cache
        /// </summary>
        /// <typeparam name="T">The type of the value that is returned</typeparam>
        /// <param name="key">The key of the entry</param>
        /// <returns></returns>
        public static T Get<T>(string key) where T : class
        {
            return MemoryCache.Default[key] as T;
        }

        /// <summary>
        /// Insert entry to the cache
        /// </summary>
        /// <param name="name">the key that will be inserted in the cache</param>
        /// <param name="value">the value that will be inserted in the cache</param>
        internal static void Set(string key, object value)
        {
            MemoryCache.Default.Set(key, value, new CacheItemPolicy());
        }
    }
}
