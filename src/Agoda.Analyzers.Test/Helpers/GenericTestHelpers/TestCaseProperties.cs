using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Reflection;

namespace Agoda.Analyzers.Test.Helpers.GenericTestHelpers
{
    /// <summary>
    /// All properties needed to execute test case
    /// </summary>
    public class TestCaseProperties
    {
        /// <summary>
        /// This is the test ID, as specified in the Github task
        /// </summary>
        public string DiagnosticId { get; }

        /// <summary>
        /// Specifies if the test will fire warning or not
        /// </summary>
        public bool IsWarning { get; }

        /// <summary>
        /// The name of the test case
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The analyzer that we are testing
        /// </summary>
        public DiagnosticAnalyzer DiagnosticAnalyzer { get; }

        /// <summary>
        /// The code descriptor that we are testing
        /// </summary>
        public CodeDescriptor CodeDescriptor { get; }

        public TestCaseProperties(string testCaseName, string testCasePrefix, string analyzersAssemblyName)
        {
            var withoutPrefix = testCaseName.Replace(testCasePrefix, String.Empty);
            var tokens = withoutPrefix.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            //The diagnostic Id is extracted from the top folder name for the test
            DiagnosticId = tokens[0];

            //The warning value is extracted from the folder name (Warning or NoWarning)
            string warningValue = tokens[1];
            IsWarning = warningValue.ToLower() == "warning";
            Name = withoutPrefix.Replace($".{DiagnosticId}.{warningValue}.", String.Empty);

            //The code that is provided in the test case
            var code = ConventionManager.GetTestCaseCode(testCaseName);

            //Analyzer that the test is using
            DiagnosticAnalyzer = ConventionManager.GetDiagnosticsFromTestCase(DiagnosticId, analyzersAssemblyName);

            //Assemblies that the test is referencing
            var referencedAssembies = AssemblyOperations.GetReferencedAssembly(DiagnosticId, typeof(GenericReferences), Assembly.GetExecutingAssembly());

            //We combine the code and the referenced assemblies to get the code descriptor
            CodeDescriptor = new CodeDescriptor();
            CodeDescriptor.Code = code;
            if (referencedAssembies != null)
                CodeDescriptor.References = referencedAssembies;
        }
    }
}
