using Agoda.Analyzers.Test.Helpers;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Agoda.Analyzers.AgodaCustom;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;


namespace Agoda.Analyzers.Test.AgodaCustom
{
    class AG0003UnitTests: DiagnosticVerifier
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

			var reference = MetadataReference.CreateFromFile(typeof(System.Web.HttpContext).Assembly.Location);

			var doc = CreateProject(new string[] { code })
				.AddMetadataReference(reference)
				.Documents
                .First();

			var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

			var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new Document[] { doc }, CancellationToken.None).ConfigureAwait(false);

            var baseResult = CSharpDiagnostic("AG0003");
            VerifyDiagnosticResults(diag, analyzersArray, new DiagnosticResult[] {
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
