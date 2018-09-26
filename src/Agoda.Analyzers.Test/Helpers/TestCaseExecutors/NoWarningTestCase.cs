using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Agoda.Analyzers.Test.Helpers.GenericTestHelpers;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Agoda.Analyzers.Test.Helpers.TestCaseExecutors
{
    public class NoWarningTestCase : GenericTestCase
    {
        public NoWarningTestCase(string code, TestCaseProperties testCaseProperties, IEnumerable<Assembly> referencedAssembies, DiagnosticAnalyzer diagnosticAnalyzer) 
            : base(code, testCaseProperties, referencedAssembies, diagnosticAnalyzer) { }

        public override async Task Execute()
        {
            var codeDescriptor = ConventionManager.GetCodeDescriptor(Code, ReferencedAssembies);
            await VerifyDiagnosticsAsync(codeDescriptor, EmptyDiagnosticResults, TestCaseProperties.DiagnosticId, DiagnosticAnalyzer);
        }
    }
}
