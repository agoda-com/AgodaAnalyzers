using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Agoda.Analyzers.Test.Helpers.GenericTestHelpers
{
    public class TestCaseProperties
    {
        public string DiagnosticId { get; }
        public bool IsWarning { get; }
        public string Name { get; }

        public DiagnosticAnalyzer DiagnosticAnalyzer { get; }

        public CodeDescriptor CodeDescriptor { get; }

        public TestCaseProperties(string testCaseName, string testCasePrefix, string analyzersAssemblyName)
        {
            var withoutPrefix = testCaseName.Replace(testCasePrefix, String.Empty);
            var tokens = withoutPrefix.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            DiagnosticId = tokens[0];
            string warningValue = tokens[1];
            IsWarning = warningValue.ToLower() == "warning";
            Name = withoutPrefix.Replace($".{DiagnosticId}.{warningValue}.", String.Empty);

            //The code that is provided in the test case
            var code = ConventionManager.GetTestCaseCode(testCaseName);

            //Analyzer that the test is using
            DiagnosticAnalyzer = ConventionManager.GetDiagnosticsFromTestCase(DiagnosticId, analyzersAssemblyName);

            //Assemblies that the test is referencing
            var referencedAssembies = AssemblyOperations.GetReferencedAssembly(DiagnosticId, typeof(GenericReferences));

            CodeDescriptor = ConventionManager.GetCodeDescriptor(code, referencedAssembies);
        }
    }
}
