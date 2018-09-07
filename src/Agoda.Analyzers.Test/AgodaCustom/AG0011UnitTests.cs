using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    class AG0011UnitTests : DiagnosticVerifier
    {
        [Test]
        public async Task TestNoDirectQueryStringAccessAsync()
        {
            var code = @"
				class TestClass {
					public void TestMethod() {
                        var queryString = System.Web.HttpContext.Current.Request.QueryString;
					}
				}
			";

            var reference = MetadataReference.CreateFromFile(typeof(HttpContext).Assembly.Location);

            var doc = CreateProject(new[] { code })
                .AddMetadataReference(reference)
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] { doc }, CancellationToken.None);
            var expected = CSharpDiagnostic(AG0011NoDirectQueryStringAccess.DIAGNOSTIC_ID).WithLocation(4, 82);

            VerifyDiagnosticResults(diag, analyzersArray, new[] { expected });
        }

        [Test]
        public async Task TestNoDirectQUeryStringAccessViaLocalVariableAsync()
        {
            var code = @"
				class TestClass {
					public void TestMethod() {
                        var request = System.Web.HttpContext.Current.Request;
                        var queryParamValue = request.QueryString[""queryParam""];
					}
				}
			";

            var reference = MetadataReference.CreateFromFile(typeof(HttpContext).Assembly.Location);

            var doc = CreateProject(new[] { code })
                .AddMetadataReference(reference)
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] { doc }, CancellationToken.None);
            var expected = CSharpDiagnostic(AG0011NoDirectQueryStringAccess.DIAGNOSTIC_ID).WithLocation(5, 55);

            VerifyDiagnosticResults(diag, analyzersArray, new[] { expected });
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0011NoDirectQueryStringAccess();
        }
    }
}