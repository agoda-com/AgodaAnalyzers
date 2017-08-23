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
    class AG0001UnitTests: DiagnosticVerifier
	{
		[Test]
		public async Task TestDependencyResolverUsageAsync()
		{
			var code = $@"
				interface ISomething {{
					void DoSomething();
				}}
			
				class TestClass {{
					public void TestMethod() {{
						var instance = System.Web.Mvc.DependencyResolver.Current.GetService(typeof(ISomething));
						//instance.DoSomething();
					}}
				}}
			";

			var reference = MetadataReference.CreateFromFile(typeof(System.Web.Mvc.DependencyResolver).Assembly.Location);

			var doc = CreateProject(new string[] { code })
				.AddMetadataReference(reference)
				.Documents
                .First();

			var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

			var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new Document[] { doc }, CancellationToken.None).ConfigureAwait(false);
            DiagnosticResult expected = CSharpDiagnostic("AG0001").WithLocation(8, 37);

			VerifyDiagnosticResults(diag, analyzersArray, new DiagnosticResult[] { expected });
		}

		protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
		{
			yield return new AG0001DependencyResolverMustNotBeUsed();
		}
	}
}
