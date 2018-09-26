using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Agoda.Analyzers.Test.Helpers.GenericTestHelpers
{
    public static class ConventionManager
    {
        public static int[] GetLocationsFromTestCase(string code)
        {
            return code.Substring(0, code.IndexOf(Environment.NewLine))
                .Replace("/*", String.Empty)
                .Replace("*/", String.Empty)
                .Trim()
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList()
                .Select(entry => Convert.ToInt32(entry))
                .ToArray();
        }

        public static DiagnosticAnalyzer GetDiagnosticsFromTestCase(string diagnosticId, string analyzersAssemblyName)
        {
            var analyzer = CacheManager.Get<DiagnosticAnalyzer>(diagnosticId);
            if (analyzer != null)
                return analyzer;

            var analyzersAssembly = CacheManager.Get<Assembly>(analyzersAssemblyName);
            if (analyzersAssembly == null)
            {
                analyzersAssembly = AssemblyOperations.GetAssembly(analyzersAssemblyName);
                CacheManager.Set(analyzersAssemblyName, analyzersAssembly);
            }

            var analyzerType = analyzersAssembly.GetTypes().Where(t =>
             t.IsSubclassOf(typeof(DiagnosticAnalyzer)) &&
             t.Name.StartsWith(diagnosticId)).FirstOrDefault();

            analyzer = (DiagnosticAnalyzer)Activator.CreateInstance(analyzerType);
            CacheManager.Set(diagnosticId, analyzer);

            return analyzer;
        }

        public static string GetTestCaseCode(string testCaseName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = testCaseName;

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new System.IO.StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static DiagnosticLocation[] GetDiagnosticLocations(int[] locations, string testCaseName)
        {
            if (locations.Length % 2 == 1)
                throw new Exception($"Location must contain line and column. Test case {testCaseName}.");

            var diagLocations = new List<DiagnosticLocation>();
            for (int i = 0; i < locations.Length; i += 2)
            {
                int line = locations[i];
                int column = locations[i + 1];
                diagLocations.Add(new DiagnosticLocation(line, column));
            }
            return diagLocations.ToArray();
        }

        public static CodeDescriptor GetCodeDescriptor(string code, IEnumerable<Assembly> assemblies)
        {
            var codeDescriptor = new CodeDescriptor();
            codeDescriptor.Code = code;

            if (assemblies != null)
                codeDescriptor.References = assemblies;

            return codeDescriptor;
        }
    }
}
