using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Agoda.Analyzers.Test.Helpers.GenericTestHelpers;

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


            if(properties.IsWarning)
            {
                //The first line of the test case needs to be locations
                var locations = ConfigManager.GetLocationsFromConfig(code);
                //The second line of the test case needs to be locations
                var assemblies = GetReferencedAssembliesFromConfig(properties.DiagnosticId);
                await ExecuteTestCaseWithWarning(properties, code, locations, assemblies, diagnosticAnalyzer);
            }
            else
            {
                //The first line of the test case needs to be references
                var assemblies = GetReferencedAssembliesFromConfig(properties.DiagnosticId);
                await ExecuteTestCaseWithNoWarning(code, assemblies, properties.DiagnosticId, diagnosticAnalyzer);
            }
        }


        private DiagnosticAnalyzer GetDiagnosticsFromTestCase(string diagnosticId)
        {
            var analyzer = CacheManager.Get<DiagnosticAnalyzer>(diagnosticId);
            if (analyzer != null)
                return analyzer;

            var analyzersAssembly = AssemblyOperations.GetAssembly(AnalyzersAssemblyName);

            var analyzerType = analyzersAssembly.GetTypes().Where(t =>
             t.IsSubclassOf(typeof(DiagnosticAnalyzer)) &&
             t.Name.StartsWith(diagnosticId)).FirstOrDefault();

            analyzer = (DiagnosticAnalyzer)Activator.CreateInstance(analyzerType);
            CacheManager.Set(diagnosticId, analyzer);

            return analyzer;
        }

        private IEnumerable<Assembly> GetReferencedAssembliesFromConfig(string diagnosticId)
        {
            var referenceType = Assembly.GetExecutingAssembly()
                .GetTypes().
                FirstOrDefault(c => c.IsSubclassOf(typeof(GenericReferences)) 
                && c.Name.StartsWith(diagnosticId));

            if (referenceType == null)
                return null;

            var referencesClass = (GenericReferences)Activator.CreateInstance(referenceType);

            var references = referencesClass.ReferenceDefinitions();
            return references.Select(r => r.Assembly);
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


        private async Task ExecuteTestCaseWithWarning(TestCaseProperties testCase, string code, int[] locations, IEnumerable<Assembly> assemblies, DiagnosticAnalyzer diagnosticAnalyzer)
        {
            var codeDescriptor = GetCodeDescriptor(code, assemblies);
            if (locations.Length % 2 == 1)
                throw new Exception($"Location must contain line and column. Test case {testCase.Name}.");

            var diagLocations = GetDiagnosticLocations(locations);
            await VerifyDiagnosticsAsync(codeDescriptor, diagLocations, testCase.DiagnosticId, diagnosticAnalyzer);
        }

        private DiagnosticLocation[] GetDiagnosticLocations(int[] locations)
        {
            var diagLocations = new List<DiagnosticLocation>();
            for (int i = 0; i < locations.Length; i += 2)
            {
                int line = locations[i];
                int column = locations[i + 1];
                diagLocations.Add(new DiagnosticLocation(line, column));
            }
            return diagLocations.ToArray();
        }

        private async Task ExecuteTestCaseWithNoWarning(string code, IEnumerable<Assembly> assemblies, string diagnosticId, DiagnosticAnalyzer diagnosticAnalyzer)
        {
            var codeDescriptor = GetCodeDescriptor(code, assemblies);
            await VerifyDiagnosticsAsync(codeDescriptor, EmptyDiagnosticResults, diagnosticId, diagnosticAnalyzer);
        }

        private CodeDescriptor GetCodeDescriptor(string code, IEnumerable<Assembly> assemblies)
        {
            var codeDescriptor = new CodeDescriptor();
            codeDescriptor.Code = code;

            if (assemblies != null)
                codeDescriptor.References = assemblies;

            return codeDescriptor;
        }

        private Assembly GetAssemblyByName(string name)
        {
            var assemblyName = Assembly.GetExecutingAssembly().
                GetReferencedAssemblies().
                FirstOrDefault(assembly => assembly.FullName.Contains(name));

            //Load the assembly
            return Assembly.Load(assemblyName.FullName);
        }
    }
}
