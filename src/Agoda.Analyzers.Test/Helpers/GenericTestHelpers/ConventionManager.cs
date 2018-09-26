using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Agoda.Analyzers.Test.Helpers.GenericTestHelpers
{
    /// <summary>
    /// The class that handles retrieving data based on convention
    /// </summary>
    public static class ConventionManager
    {
        /// <summary>
        /// Extracts the location from the test case file
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static int[] GetLocationsFromTestCase(string code)
        {
            //The location is specified as a first line in the test case
            return code.Substring(0, code.IndexOf(Environment.NewLine))
                .Replace("/*", String.Empty)
                .Replace("*/", String.Empty)
                .Trim()
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList()
                .Select(entry => Convert.ToInt32(entry))
                .ToArray();
        }

        /// <summary>
        /// Retrieves the DiagnosticAnalizer for the specified test case
        /// </summary>
        /// <param name="diagnosticId">The diagnostic id of the test case</param>
        /// <param name="analyzersAssemblyName">The name of the assembly that contains the Diagnostic analyzers</param>
        /// <returns></returns>
        public static DiagnosticAnalyzer GetDiagnosticsFromTestCase(string diagnosticId, string analyzersAssemblyName)
        {
            var analyzer = CacheManager.Get<DiagnosticAnalyzer>(diagnosticId);
            if (analyzer != null)
                return analyzer;

            //Extracts the assembly that contains the analyzer
            var analyzersAssembly = CacheManager.Get<Assembly>(analyzersAssemblyName);
            if (analyzersAssembly == null)
            {
                analyzersAssembly = AssemblyOperations.GetAssembly(analyzersAssemblyName);
                CacheManager.Set(analyzersAssemblyName, analyzersAssembly);
            }

            //Extracts the analyzer type
            var analyzerType = analyzersAssembly.GetTypes().Where(t =>
             t.IsSubclassOf(typeof(DiagnosticAnalyzer)) &&
             t.Name.StartsWith(diagnosticId)).FirstOrDefault();

            //Creates instance of the analyzer
            analyzer = (DiagnosticAnalyzer)Activator.CreateInstance(analyzerType);
            CacheManager.Set(diagnosticId, analyzer);

            return analyzer;
        }

        /// <summary>
        /// Extracts the code of the test case when given the name of the test case
        /// </summary>
        /// <param name="testCaseName">Name of the test case</param>
        /// <returns>The code of the test case</returns>
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

        /// <summary>
        /// Convert int[] to DiagnosticLocation[] for the specific test case
        /// </summary>
        /// <param name="locations">Locations where warning should happen</param>
        /// <param name="testCaseName">The name of the specific test case</param>
        /// <returns></returns>
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
    }
}
