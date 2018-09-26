using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Agoda.Analyzers.Test.Helpers.GenericTestHelpers;
using Agoda.Analyzers.Test.Helpers.TestCaseExecutors;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    public class GenericUnitTest
    {
        const string AnalyzersAssemblyName = "Agoda.Analyzers";
        const string TestCasePrefix = "Agoda.Analyzers.Test.AgodaCustom.TestCases";

        /// <summary>
        /// Reads all test cases (marked as embedded resources) in the current assembly
        /// </summary>
        /// <returns>List of test cases names</returns>
        public static IEnumerable<string> GetAllTestCases()
        {
            return AssemblyOperations.GetEmbeddedResourcesByPrefix(TestCasePrefix);
        }

        /// <summary>
        /// Execute generic test based on convention
        /// </summary>
        /// <param name="testCaseName"></param>
        /// <returns></returns>
        [Test, TestCaseSource("GetAllTestCases")]
        public async Task GenericTest(string testCaseName)
        {
            //The code that is provided in the test case
            var code = ConventionManager.GetTestCaseCode(testCaseName);

            //Test properties for the test
            var properties = new TestCaseProperties(testCaseName, TestCasePrefix);

            //Analyzer that the test is using
            var diagnosticAnalyzer = ConventionManager.GetDiagnosticsFromTestCase(properties.DiagnosticId, AnalyzersAssemblyName);

            //Assemblies that the test is referencing
            var refAssemblies = AssemblyOperations.GetReferencedAssembly(properties.DiagnosticId, typeof(GenericReferences));

            GenericTestCase testCase = (properties.IsWarning) ? 
                new WarningTestCase(code, properties, refAssemblies, diagnosticAnalyzer) as GenericTestCase :
                new NoWarningTestCase(code, properties, refAssemblies, diagnosticAnalyzer);

            await testCase.Execute();
        }
    }
}
