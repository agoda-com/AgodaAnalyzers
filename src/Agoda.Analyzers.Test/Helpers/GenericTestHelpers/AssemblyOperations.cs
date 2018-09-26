using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Agoda.Analyzers.Test.Helpers.GenericTestHelpers
{
    /// <summary>
    /// Operations for Assemblies
    /// </summary>
    public static class AssemblyOperations
    {
        /// <summary>
        /// Get the concrete assembly by name. Throws exception if it doesn't exists.
        /// </summary>
        /// <param name="name">The name of the assembly</param>
        /// <returns>The assembly instance</returns>
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

        /// <summary>
        /// Retrieves all referenced assemblies by filter
        /// </summary>
        /// <param name="classNamePrefix">The prefix of the class that is contained in the assembly</param>
        /// <param name="parentType">The parent type of the specified class</param>
        /// <param name="assembly">The assembly that we are searching in</param>
        /// <returns></returns>
        public static IEnumerable<Assembly> GetReferencedAssembly(string classNamePrefix, Type parentType, Assembly assembly)
        {
            var referenceType = assembly.GetTypes().
                FirstOrDefault(c => c.IsSubclassOf(parentType)
                && c.Name.StartsWith(classNamePrefix));

            if (referenceType == null)
                return null;

            var referencesClass = (GenericReferences)Activator.CreateInstance(referenceType);

            var references = referencesClass.ReferenceDefinitions;
            return references.Select(r => r.Assembly);
        }

        /// <summary>
        /// Retrieves all the embedded recources with specified prefix
        /// </summary>
        /// <param name="embeddedResourcePrefix">The prefix that is resources must have</param>
        /// <returns></returns>
        public static IEnumerable<string> GetEmbeddedResourcesByPrefix(string embeddedResourcePrefix)
        {
            var embeddedResources = Assembly.GetExecutingAssembly().GetManifestResourceNames().ToList();
            return embeddedResources.Where(name => name.StartsWith(embeddedResourcePrefix));
        }
    }
}
