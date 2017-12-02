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
    class AG0002UnitTests : DiagnosticVerifier
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

            var doc = CreateProject(new[] {code})
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] {doc}, CancellationToken.None).ConfigureAwait(false);

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

            var doc = CreateProject(new[] {code})
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] {doc}, CancellationToken.None).ConfigureAwait(false);

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

            var doc = CreateProject(new[] {code})
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] {doc}, CancellationToken.None).ConfigureAwait(false);

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

            var doc = CreateProject(new[] {code})
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] {doc}, CancellationToken.None).ConfigureAwait(false);
            var expected = CSharpDiagnostic("AG0002").WithLocation(10, 21);

            VerifyDiagnosticResults(diag, analyzersArray, new[] {expected});
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0002PrivateMethodsShouldNotBeTested();
        }
    }
}