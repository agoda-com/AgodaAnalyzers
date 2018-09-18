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
            var code = @"
using System.Collections;
using NUnit.Framework;

namespace Tests
{
    public class TestClass
    {
        [Test]
        public void This_IsValid(){}

        [Test]
        public void This_Is_Valid(){}

        [Test]
        public void This_1s_Valid(){}

        [Test]
        public void This_IsAlso_QuiteValid555(){}

        [Test]
        public void ThisIsInvalid(){} // no underscores, so invalid

        [TestCase]
        public void This_Is_In_Valid(){} // too many underscores, so invalid

        [TestCaseSource(typeof(MyTestDataClass), ""TestCases"")]
        public void This_Is_invalid(){} // incorrect casing, so invalid

        public void ThisIsNotATest(){}  // not a test = valid

        [Test]
        private void PrivateMethod(){} // private = so valid

        [Test]
        protected void ProtectedMethod(){} // protected = valid

        [Test]
        internal void InternalMethod(){} // internal = valid
	}

    public class MyTestDataClass
    {
        public static IEnumerable TestCases
        {
            get
            {
                yield return null;
            }
        }  
    }
}";

            var nUnit = MetadataReference.CreateFromFile(typeof(TestFixtureAttribute).Assembly.Location);

            var doc = CreateProject(new[] { code })
                .AddMetadataReference(nUnit)
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] { doc }, CancellationToken.None).ConfigureAwait(false);

            var baseResult = CSharpDiagnostic(AG0005TestMethodNamesMustFollowConvention.DIAGNOSTIC_ID);
            VerifyDiagnosticResults(diag, analyzersArray, new[]
            {
                baseResult.WithLocation(21, 9),
                baseResult.WithLocation(24, 9),
                baseResult.WithLocation(27, 9),
            });
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0005TestMethodNamesMustFollowConvention();
        }
    }
}