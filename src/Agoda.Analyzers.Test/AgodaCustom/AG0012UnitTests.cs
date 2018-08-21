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
    internal class AG0012UnitTests : DiagnosticVerifier
    {
        [Test]
        public async Task TestTestMethodNamesMustContainAtLeastOneAssertion()
        {
            var code = $@"
using NUnit.Framework;

namespace Tests
{{
    public class TestClass
    {{
        [Test]
        public void This_Is_Valid(){{
            int[] array = new int[] {{ 1,2, 3 }};
            Assert.That(array, Has.Exactly(1).EqualTo(3));
        }}

        [Test]
        public void This_Is_Not_Valid(){{
            int[] array = new int[] {{ 1,2, 3 }};
        }}
	}}
}}";

            var nUnit = MetadataReference.CreateFromFile(typeof(TestFixtureAttribute).Assembly.Location);

            var doc = CreateProject(new[] { code })
                .AddMetadataReference(nUnit)
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] { doc }, CancellationToken.None).ConfigureAwait(false);            
            var baseResult = CSharpDiagnostic(AG0012TestMethodMustContainAtLeastOneAssertion.DiagnosticId);

            VerifyDiagnosticResults(diag, analyzersArray, new[]
            {
                baseResult.WithLocation(14, 9),
            });
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0012TestMethodMustContainAtLeastOneAssertion();
        }
    }
}