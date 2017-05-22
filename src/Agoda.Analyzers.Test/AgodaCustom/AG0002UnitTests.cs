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
    class AG0002UnitTests: DiagnosticVerifier
	{
		[Test]
		public async Task TestCorrectDeclarationShouldNotCauseAnyIssue()
		{
			var code = $@"
				interface ISomething {{
					void DoSomething();
				}}
			
				class TestClass : ISomething {{
					public void DoSomething() {{
					}}
				}}
			";

			var doc = CreateProject(new string[] { code })
				.Documents
                .First();

			var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

			var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new Document[] { doc }, CancellationToken.None).ConfigureAwait(false);

			VerifyDiagnosticResults(diag, analyzersArray, new DiagnosticResult[] { });
		}

        [Test]
        public async Task TestExtraPublicDeclarationShouldNotCauseAnyIssue()
        {
            var code = $@"
				interface ISomething {{
					void DoSomething();
				}}
			
				class TestClass : ISomething {{
					public void DoSomething() {{
					}}
                    public void DoSomething2() {{
					}}
				}}
			";

            var doc = CreateProject(new string[] { code })
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new Document[] { doc }, CancellationToken.None).ConfigureAwait(false);

            VerifyDiagnosticResults(diag, analyzersArray, new DiagnosticResult[] { });
        }


        [Test]
        public async Task TestExplicitInterfaceImplementationShouldNotCauseAnyError()
        {
            var code = $@"
				interface ISomething {{
					void DoSomething();
				}}
			
				class TestClass : ISomething {{
					void ISomething.DoSomething() {{
					}}
				}}
			";

            var doc = CreateProject(new string[] { code })
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new Document[] { doc }, CancellationToken.None).ConfigureAwait(false);

            VerifyDiagnosticResults(diag, analyzersArray, new DiagnosticResult[] { });
        }

        [Test]
        public async Task TestMethodThatNotPartOfTheInterfaceShouldNotBeInternal()
        {
            var code = $@"
				interface ISomething {{
					void DoSomething();
				}}
			
				class TestClass : ISomething {{
					public void DoSomething() {{

					}}
                    internal void DoSomething2() {{

					}}
				}}
			";

            var doc = CreateProject(new string[] { code })
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new Document[] { doc }, CancellationToken.None).ConfigureAwait(false);
            DiagnosticResult expected = CSharpDiagnostic("AG0002").WithLocation(10, 21);

            VerifyDiagnosticResults(diag, analyzersArray, new DiagnosticResult[] { expected });
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
		{
			yield return new AG0002PrivateMethodsShouldNotBeTested();
		}
	}
}
