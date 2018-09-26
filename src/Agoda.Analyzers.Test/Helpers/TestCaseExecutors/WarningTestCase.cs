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

        public WarningTestCase(TestCaseProperties testCaseProperties) : base(testCaseProperties)
        {
            //The first line of the test case needs to be locations
            locations = ConventionManager.GetLocationsFromTestCase(TestCaseProperties.CodeDescriptor.Code);
        }

        public override async Task Execute()
        {
            var diagLocations = ConventionManager.GetDiagnosticLocations(locations, TestCaseProperties.Name);
            await VerifyDiagnosticsAsync(TestCaseProperties.CodeDescriptor, diagLocations, TestCaseProperties.DiagnosticId, TestCaseProperties.DiagnosticAnalyzer);
        }
    }
}
