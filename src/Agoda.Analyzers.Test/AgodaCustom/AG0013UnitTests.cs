using System.Linq;
using NUnit.Framework;
using System.Threading;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using System.Collections.Generic;
using Agoda.Analyzers.AgodaCustom;
using System.Collections.Immutable;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    class AG0013UnitTests : DiagnosticVerifier
    {
        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0013LimitNumberOfTestMethodParametersTo5();
        }

        [Test]
        public async Task TestMethod_ShouldNotTake_MoreThan5Inputs()
        {
            var testCode = @"
            using NUnit.Framework;

            namespace Test
            {
                [TestFixture]
                public class Test_Method
                {
                    [Test]
                    [TestCase(0, true)]
                    [TestCase(1, true)]
                    public void This_Is_Valid(int number, bool expected) { }

                    [Test]
                    [TestCase(0, true)]
                    [TestCase(1, true)]
                    [TestCase(2, true)]
                    [TestCase(3, true)]
                    [TestCase(4, true)]
                    [TestCase(5, true)]
                    public void This_Is_NotValid(int number, bool expected) { }

                    [Test]
                    [TestCase(0, true), TestCase(1, true), TestCase(2, true)]
                    [TestCase(3, true), TestCase(4, true), TestCase(5, true)]
                    public void This_Is_NotValidAsWell(int number, bool expected) { }
                }
            }";

            var analyzer = GetCSharpDiagnosticAnalyzers().ToImmutableArray();
            var document = CreateProject(new[] { testCode }).
                           AddMetadataReference(MetadataReference.CreateFromFile(typeof(TestFixtureAttribute).Assembly.Location)).
                           Documents.
                           First();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzer, new[] { document }, CancellationToken.None).ConfigureAwait(false);

            var baseResult = CSharpDiagnostic(AG0013LimitNumberOfTestMethodParametersTo5.DIAGNOSTIC_ID);
            VerifyDiagnosticResults(diag, analyzer, new[]
            {
                baseResult.WithLocation(14, 21),
                baseResult.WithLocation(23, 21),
            });
        }
    }
}
