using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Agoda.Analyzers.Test.Helpers.GenericTestHelpers
{
    public static class CacheManager
    {
        public static T Get<T>(string key) where T : class
        {
            return MemoryCache.Default[key] as T;
        }

        internal static void Set(string name, object value)
        {
            MemoryCache.Default.Set(name, value, new CacheItemPolicy());
        }
    }
}
