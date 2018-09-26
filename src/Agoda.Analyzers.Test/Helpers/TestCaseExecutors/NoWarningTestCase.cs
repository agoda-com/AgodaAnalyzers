using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Agoda.Analyzers.Test.Helpers.GenericTestHelpers;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Agoda.Analyzers.Test.Helpers.TestCaseExecutors
{
    /// <summary>
    /// Test case that will not fire warning
    /// </summary>
    public class NoWarningTestCase : GenericTestCase
    {
        public NoWarningTestCase(TestCaseProperties testCaseProperties) 
            : base(testCaseProperties) { }

        public override async Task Execute()
        {
            await VerifyDiagnosticsAsync(TestCaseProperties.CodeDescriptor, EmptyDiagnosticResults, TestCaseProperties.DiagnosticId, TestCaseProperties.DiagnosticAnalyzer);
        }
    }
}
