using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Runtime.Caching;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    public class GenericUnitTest : DiagnosticVerifier
    {

        const string AnalyzersAssemblyName = "Agoda.Analyzers";
        const string TestCasePrefix = "Agoda.Analyzers.Test.AgodaCustom.TestCases";
        
        [Test, TestCaseSource("GetAllTestCases")]
        public async Task GenericTest(string testCaseName)
        {
            var code = GetTestCaseCode(testCaseName);
            var properties = new TestCaseProperties(testCaseName, TestCasePrefix);

            var diagnosticAnalyzer = GetDiagnosticsFromTestCase(properties.DiagnosticId);

            await ((properties.IsWarning) ?
                ExecuteTestCaseWithWarning(properties, code, GetLocationsFromConfig(code), diagnosticAnalyzer) 
                : ExecuteTestCaseWithNoWarning(code, properties.DiagnosticId, diagnosticAnalyzer));
        }

        private Assembly GetAnalyzersAssembly()
        {
            string analyzersAssemblyCacheKey = "AnalyzersAssembly";

            var analyzersAssembly = MemoryCache.Default[analyzersAssemblyCacheKey] as Assembly;
            if (analyzersAssembly != null)
                return analyzersAssembly;

            //Check if assemby exists (this should be new unit test)
            var assemblyName = Assembly.GetExecutingAssembly().
                GetReferencedAssemblies().
                Where(a => a.Name == AnalyzersAssemblyName)
                .FirstOrDefault();

            if (assemblyName == null)
                throw new Exception("Assembly doesn't exists");

            //Load the analyzer assembly
            analyzersAssembly = Assembly.Load(assemblyName.FullName);
            MemoryCache.Default.Set(analyzersAssemblyCacheKey, analyzersAssembly, new CacheItemPolicy());

            return analyzersAssembly;
        }

        private DiagnosticAnalyzer GetDiagnosticsFromTestCase(string diagnosticId)
        {
            var analyzer = MemoryCache.Default[diagnosticId] as DiagnosticAnalyzer;
            if (analyzer != null)
                return analyzer;

            var analyzersAssembly = GetAnalyzersAssembly();

            var analyzerType = analyzersAssembly.GetTypes().Where(t =>
             t.IsSubclassOf(typeof(DiagnosticAnalyzer)) &&
             t.Name.StartsWith(diagnosticId)).FirstOrDefault();

            analyzer = (DiagnosticAnalyzer)Activator.CreateInstance(analyzerType);
            MemoryCache.Default[diagnosticId] = analyzer;

            return analyzer;
        }

        private int[] GetLocationsFromConfig(string code)
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
        
        public static List<string> GetAllTestCases()
        {
            var embeddedResources = Assembly.GetExecutingAssembly().GetManifestResourceNames().ToList();
            return embeddedResources.Where(name => name.StartsWith(TestCasePrefix)).ToList();
        }

        public string GetTestCaseCode(string testCaseName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = testCaseName;

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new System.IO.StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }


        private async Task ExecuteTestCaseWithWarning(TestCaseProperties testCase, string code, int[] locations, DiagnosticAnalyzer diagnosticAnalyzer)
        {
            if (locations.Length % 2 == 1)
                throw new System.Exception($"Location must contain line and column. Test case {testCase.Name}.");

            for (int i = 0; i < locations.Length; i += 2)
            {
                int line = locations[i];
                int column = locations[i + 1];
                await VerifyDiagnosticsAsync(code, new DiagnosticLocation(line, column), testCase.DiagnosticId, diagnosticAnalyzer);
            }
        }

        private async Task ExecuteTestCaseWithNoWarning(string code, string diagnosticId, DiagnosticAnalyzer diagnosticAnalyzer)
        {
            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults, diagnosticId, diagnosticAnalyzer);
        }
    }

    public class TestCaseProperties
    {
        public string DiagnosticId { get; }
        public bool IsWarning { get; }
        public string Name { get; }

        public TestCaseProperties(string testName, string testCasePrefix)
        {
            var withoutPrefix = testName.Replace(testCasePrefix, String.Empty);
            var tokens = withoutPrefix.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            DiagnosticId = tokens[0];
            string warningValue = tokens[1];
            IsWarning = warningValue.ToLower() == "warning";
            Name = withoutPrefix.Replace($".{DiagnosticId}.{warningValue}.", String.Empty);
        }
    }
}
