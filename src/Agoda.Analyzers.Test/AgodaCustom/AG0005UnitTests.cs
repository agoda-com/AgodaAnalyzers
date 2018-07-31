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
    internal class AG0005UnitTests : DiagnosticVerifier
    {
        [Test]
        public async Task TestTestMethodNamesMustFollowConvention()
        {
            var code = $@"
using NUnit.Framework;

namespace Tests
{{
    public class TestClass
    {{
        [Test]
        public void This_IsValid(){{}} // should validate

        [Test]
        public void This_Is_Valid(){{}} // should validate

        [Test]
        public void This_1s_Valid(){{}} // should validate

        [Test]
        public void This_IsAlso_QuiteValid555(){{}} // should validate

        [Test]
        public void ThisIsInvalid(){{}} // no underscores, so should not validate

        [Test]
        public void This_Is_In_Valid(){{}} // too many underscores, so should not validate

        [Test]
        public void This_Is_invalid(){{}} // incorrect casing, so should not validate

        public void ThisIsNotATest(){{}}  // not a test, so should validate

        [Test]
        private void Is_This_EvenPossible(){{}} // private, so should validate
	}}
}}";

            var nUnit = MetadataReference.CreateFromFile(typeof(NUnit.Framework.TestFixtureAttribute).Assembly.Location);

            var doc = CreateProject(new[] { code })
                .AddMetadataReference(nUnit)
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] { doc }, CancellationToken.None).ConfigureAwait(false);

            var baseResult = CSharpDiagnostic(AG0005TestMethodNamesMustFollowConvention.DiagnosticId);
            VerifyDiagnosticResults(diag, analyzersArray, new[]
            {
                baseResult.WithLocation(20, 9),
                baseResult.WithLocation(23, 9),
                baseResult.WithLocation(26, 9),
            });
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0005TestMethodNamesMustFollowConvention();
        }
    }
}