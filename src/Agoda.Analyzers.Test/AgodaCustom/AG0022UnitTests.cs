using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    internal class AG0022UnitTests : DiagnosticVerifier
    {
        [Test]
        public async Task AG0022_WhenNotExistBothSyncAndAsyncVersionsOfMethods_ShouldNotShowWarning()
        {
            var code = @"
				using System.Threading.Tasks;

                interface Interface1
                {
                    bool TestMethod(string url);
                    Task<bool> TestMethod2Async(string url);
                }
			";

            await TestForResults(code);
        }

   //     [Test]
   //     public async Task AG0022_WhenExistBothSyncAndAsyncVersionsOfMethods_ShouldShowWarning()
   //     {
   //         var code = @"
			//	interface Interface
   //             {
   //                 bool TestMethod(string url);
   //                 Task<bool> TestMethodAsync(string url);
   //             }
			//";

   //         var baseResult =
   //             CSharpDiagnostic(AG0022DoNotExposeBothSyncAndAsyncVersionsOfMethods.DiagnosticId);
   //         var expected = new[]
   //         {
   //             baseResult.WithLocation(4, 3)
   //         };

   //         await TestForResults(code, expected);
   //     }

        private async Task TestForResults(string code, DiagnosticResult[] expected = null)
        {
            expected = expected ?? new DiagnosticResult[0];
            var reference = MetadataReference.CreateFromFile(typeof(Task).Assembly.Location);

            var doc = CreateProject(new[] {code})
                .AddMetadataReference(reference)
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diagnostics = 
                await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] {doc}, CancellationToken.None)
                     .ConfigureAwait(false);

            VerifyDiagnosticResults(diagnostics, analyzersArray, expected);
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0022DoNotExposeBothSyncAndAsyncVersionsOfMethods();
        }
    }
}
