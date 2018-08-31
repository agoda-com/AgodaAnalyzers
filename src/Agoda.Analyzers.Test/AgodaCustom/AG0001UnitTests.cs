using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    class AG0001UnitTests : DiagnosticVerifier
    {
        [Test]
        public async Task TestDependencyResolverUsageAsync()
        {
            var code = @"
				interface ISomething {
					void DoSomething();
				}
			
				class TestClass {
					public void TestMethod() {
						var instance = System.Web.Mvc.DependencyResolver.Current.GetService(typeof(ISomething));
						//instance.DoSomething();
					}
				}
			";

            var reference = MetadataReference.CreateFromFile(typeof(DependencyResolver).Assembly.Location);

            var doc = CreateProject(new[] {code})
                .AddMetadataReference(reference)
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] {doc}, CancellationToken.None).ConfigureAwait(false);
            var expected = CSharpDiagnostic(AG0001DependencyResolverMustNotBeUsed.DiagnosticId).WithLocation(8, 37);

            VerifyDiagnosticResults(diag, analyzersArray, new[] {expected});
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0001DependencyResolverMustNotBeUsed();
        }
    }
}