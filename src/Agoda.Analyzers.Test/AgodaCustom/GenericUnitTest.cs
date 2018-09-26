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
            //Get the properties for the test case
            var testCaseProperties = new TestCaseProperties(testCaseName, TestCasePrefix, AnalyzersAssemblyName);

            //Get the test case type based on the folder name it is in
            var testCase = (testCaseProperties.IsWarning) ? 
                new WarningTestCase(testCaseProperties) as GenericTestCase :
                new NoWarningTestCase(testCaseProperties);

            //Execute the test case
            await testCase.Execute();
        }
    }
}
