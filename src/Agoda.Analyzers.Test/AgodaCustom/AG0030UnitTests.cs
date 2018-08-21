using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    internal class AG0030UnitTests : DiagnosticVerifier
    {
        [Test]
        public async Task AG0030_WhenNoDynamic_ShouldntShowAnyWarning()
        {
            var code = $@"
				class TestClass {{
					public void TestMethod1() {{
						int instance = 1;
					}}

                    public int TestMethod2() {{
						return 1;
					}}
				}}
			";

            await TestForResults(code);
        }
        
        [Test]
        public async Task AG0030_WhenMethodReturnsDynamic_ShowWarning()
        {
            var code = $@"
				class TestClass {{
					public dynamic TestMethod2() {{
						return 1;
					}}
				}}
			";

            var baseResult =
                CSharpDiagnostic(AG0030PreventUseOfDynamics.DiagnosticId);
            var expected = new[]
            {
                baseResult.WithLocation(3, 6)
            };
            await TestForResults(code, expected);
        }
        
        [Test]
        public async Task AG0030_WhenDynamicVariableDeclared_ShowWarning()
        {
            var code = $@"
				class TestClass {{
					public void TestMethod1() {{
						dynamic instance = 1;
					}}
				}}
			";

            var baseResult =
                CSharpDiagnostic(AG0030PreventUseOfDynamics.DiagnosticId);
            var expected = new[]
            {
                baseResult.WithLocation(4, 7)
            };
            await TestForResults(code, expected);
        }
        
        [Test]
        public async Task AG0030_WhenMultipleDynamicUsed_ShowWarning()
        {
            var code = $@"
				class TestClass {{
					public dynamic TestMethod2() {{
						return 1;
					}}

					public void TestMethod1() {{
						dynamic instance = 1;
					}}
				}}
			";

            var baseResult =
                CSharpDiagnostic(AG0030PreventUseOfDynamics.DiagnosticId);
            var expected = new[]
            {
                baseResult.WithLocation(3, 6),
                baseResult.WithLocation(8, 7)
            };
            await TestForResults(code, expected);
        }

        private async Task TestForResults(string code, DiagnosticResult[] expected = null)
        {
            expected = expected ?? new DiagnosticResult[0];
            var doc = CreateProject(new[] {code})
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] {doc}, CancellationToken.None)
                .ConfigureAwait(false);


            VerifyDiagnosticResults(diag, analyzersArray, expected);
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0030PreventUseOfDynamics();
        }
    }
}