using Agoda.Analyzers.Test.Helpers.GenericTestHelpers;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Agoda.Analyzers.Test.Helpers.TestCaseExecutors
{
    public class WarningTestCase : GenericTestCase
    {
        private int[] locations;

        public WarningTestCase(string code, TestCaseProperties testCaseProperties, IEnumerable<Assembly> referencedAssemblies, DiagnosticAnalyzer diagnosticAnalyzer) : 
            base(code, testCaseProperties, referencedAssemblies, diagnosticAnalyzer)
        {
            //The first line of the test case needs to be locations
            locations = ConventionManager.GetLocationsFromTestCase(code);
        }

        public override async Task Execute()
        {
            var codeDescriptor = ConventionManager.GetCodeDescriptor(Code, ReferencedAssembies);
            var diagLocations = ConventionManager.GetDiagnosticLocations(locations, TestCaseProperties.Name);
            await VerifyDiagnosticsAsync(codeDescriptor, diagLocations, TestCaseProperties.DiagnosticId, DiagnosticAnalyzer);
        }
    }
}
