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
    class AG0003UnitTests : DiagnosticVerifier
    {
        [Test]
        public async Task TestHttpContextAsArgument()
        {
            var code = $@"
                using System.Web;

				interface ISomething {{
					void SomeMethod(HttpContext c, string sampleString); // ugly interface method
				}}
			
				class TestClass: ISomething {{
                    
                    public void SomeMethod(HttpContext context, string sampleString) {{
                        // this method is ugly
                    }}

					public TestClass(System.Web.HttpContext context) {{
                        // this constructor is uglier
					}}
				}}
			";

            var reference = MetadataReference.CreateFromFile(typeof(HttpContext).Assembly.Location);

            var doc = CreateProject(new[] {code})
                .AddMetadataReference(reference)
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] {doc}, CancellationToken.None).ConfigureAwait(false);

            var baseResult = CSharpDiagnostic("AG0003");
            VerifyDiagnosticResults(diag, analyzersArray, new[]
            {
                baseResult.WithLocation(5, 22),
                baseResult.WithLocation(10, 44),
                baseResult.WithLocation(14, 23)
            });
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0003HttpContextCannotBePassedAsMethodArgument();
        }
    }
}