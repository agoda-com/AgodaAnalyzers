using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Agoda.Analyzers.Test.Helpers.GenericTestHelpers
{
    public static class AssemblyOperations
    {
        public static Assembly GetAssembly(string name)
        {
            var assembly = CacheManager.Get<Assembly>(name);
            if (assembly != null)
                return assembly;

            //Check if assemby exists (this should be new unit test)
            var assemblyName = Assembly.GetExecutingAssembly().
                GetReferencedAssemblies().
                Where(a => a.Name == name)
                .FirstOrDefault();

            if (assemblyName == null)
                throw new Exception("Assembly doesn't exists");

            //Load the analyzer assembly
            assembly = Assembly.Load(assemblyName.FullName);
            CacheManager.Set(name, assembly);

            return assembly;
        }
    }
}
