using Agoda.Analyzers.Test.Helpers;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using Agoda.Analyzers.Test.Helpers.GenericTestHelpers;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    public class GenericUnitTest : DiagnosticVerifier
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
            //Get the properties for the test case
            var testCaseProperties = new TestCaseProperties(testCaseName, TestCasePrefix, AnalyzersAssemblyName);
            await Execute(testCaseProperties);
        }

        /// <summary>
        /// Executing the test case
        /// </summary>
        /// <param name="testCaseProperties">Properties for execution of the test case</param>
        /// <returns>Task for async execution</returns>
        private async Task Execute(TestCaseProperties testCaseProperties)
        {
            await ((testCaseProperties.IsWarning) ?
               ExecuteWarningTestCase(testCaseProperties):
                ExecuteNonWarningTestCase(testCaseProperties));
        }

        /// <summary>
        /// Executing warning test case
        /// </summary>
        /// <param name="testCaseProperties">Properties for execution of the test case</param>
        /// <returns>Task for async execution</returns>
        private async Task ExecuteWarningTestCase(TestCaseProperties testCaseProperties)
        {
            //The first line of the test case needs to be locations
            var locations = ConventionManager.GetLocationsFromTestCase(testCaseProperties.CodeDescriptor.Code);
            var diagLocations = ConventionManager.GetDiagnosticLocations(locations, testCaseProperties.Name);
            await VerifyDiagnosticsAsync(testCaseProperties.CodeDescriptor, diagLocations, testCaseProperties.DiagnosticId, testCaseProperties.DiagnosticAnalyzer);
        }

        /// <summary>
        /// Executing non warning test case
        /// </summary>
        /// <param name="testCaseProperties">Properties for execution of the test case</param>
        /// <returns>Task for async execution</returns>
        private async Task ExecuteNonWarningTestCase(TestCaseProperties testCaseProperties)
        {
            await VerifyDiagnosticsAsync(testCaseProperties.CodeDescriptor, EmptyDiagnosticResults, testCaseProperties.DiagnosticId, testCaseProperties.DiagnosticAnalyzer);
        }
    }
}
