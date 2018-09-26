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
            //Check if assemby exists (this should be new unit test)
            var assemblyName = Assembly.GetExecutingAssembly().
                GetReferencedAssemblies().
                Where(a => a.Name == name)
                .FirstOrDefault();

            if (assemblyName == null)
                throw new Exception("Assembly doesn't exists");

            //Load the analyzer assembly
            return Assembly.Load(assemblyName.FullName);
        }

        public static IEnumerable<Assembly> GetReferencedAssembly(string name, Type parentClass)
        {
            var referenceType = Assembly.GetExecutingAssembly()
                .GetTypes().
                FirstOrDefault(c => c.IsSubclassOf(parentClass)
                && c.Name.StartsWith(name));

            if (referenceType == null)
                return null;

            var referencesClass = (GenericReferences)Activator.CreateInstance(referenceType);

            var references = referencesClass.ReferenceDefinitions();
            return references.Select(r => r.Assembly);
        }

        public static IEnumerable<string> GetEmbeddedResourcesByPrefix(string embeddedResourcePrefix)
        {
            var embeddedResources = Assembly.GetExecutingAssembly().GetManifestResourceNames().ToList();
            return embeddedResources.Where(name => name.StartsWith(embeddedResourcePrefix));
        }
    }
}
