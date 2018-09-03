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
    internal class AG0025UnitTests : DiagnosticVerifier
    {
        [Test]
        public async Task AG0025_WhenInvokeStart_ShouldNotShowWarning()
        {
            var code = @"
                using System.Threading.Tasks;

				class TestClass {
					public void TestMethod1() {
						new Task(() => {}).Start();
					}
				}
			";
            
            await TestForResults(code);
        }

        [Test]
        public async Task AG0025_WhenInvokeCustomTaskContinue_ShouldNotShowWarning()
        {
            var code = @"
                class Task {
                    public void Continue() {}
                }

				class TestClass {
					public void TestMethod1() {
						new Task().Continue();
					}
				}
			";

            await TestForResults(code);
        }

        [Test]
        public async Task AG0025_WhenInvokeContinueWith_ShouldShowWarning()
        {
            var code = @"
                using System.Threading.Tasks;

				class TestClass {
					public void TestMethod1() {
						new Task(() => {}).ContinueWith(null);
					}
				}
			";

            var baseResult = CSharpDiagnostic(AG0025PreventUseOfTaskContinue.DiagnosticId);
            var expected = new[]
            {
                baseResult.WithLocation(6, 26)
            };

            await TestForResults(code, expected);
        }

        [Test]
        public async Task AG0025_WhenInvokeContinueGeneric_ShouldShowWarning()
        {
            var code = @"
                using System.Threading.Tasks;

				class TestClass {
					public void TestMethod1() {
						new Task(() => {}).ContinueWith<string>(null);
					}
				}
			";
            
            var baseResult = CSharpDiagnostic(AG0025PreventUseOfTaskContinue.DiagnosticId);
            var expected = new[]
            {
                baseResult.WithLocation(6, 26)
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
            yield return new AG0025PreventUseOfTaskContinue();
        }
    }
}