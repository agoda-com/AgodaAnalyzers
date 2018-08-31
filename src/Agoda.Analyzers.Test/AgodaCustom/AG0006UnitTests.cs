using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    internal class AG0006UnitTests : DiagnosticVerifier
    {
        [Test]
        public async Task ClassShouldHaveOnlyOnePublicConstructor_ShouldNotShowWarningForZeroPublicConstructor()
        {
            var code = $@"
namespace Agoda.Analyzers.Test
{{
    public class TestClass
    {{
        private int value;

        private TestClass()
        {{
            value = 0;
        }}
    }}
}}";

            var doc = CreateProject(new[] {code})
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] {doc}, CancellationToken.None)
                .ConfigureAwait(false);

            CSharpDiagnostic(AG0006ClassShouldNotHaveMoreThanOnePublicConstructor.DiagnosticId);
            VerifyDiagnosticResults(diag, analyzersArray, new DiagnosticResult[0]);
        }

        [Test]
        public async Task ClassShouldHaveOnlyOnePublicConstructor_ShouldNotShowWarningForOnePublicConstructor()
        {
            var code = $@"
namespace Agoda.Analyzers.Test
{{
    public class TestClass
    {{
        private int value;

        public TestClass()
        {{
            value = 0;
        }}
    }}
}}";

            var doc = CreateProject(new[] {code})
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] {doc}, CancellationToken.None)
                .ConfigureAwait(false);

            CSharpDiagnostic(AG0006ClassShouldNotHaveMoreThanOnePublicConstructor.DiagnosticId);
            VerifyDiagnosticResults(diag, analyzersArray, new DiagnosticResult[0]);
        }

        [Test]
        public async Task ClassShouldHaveOnlyOnePublicConstructor_ShouldShowWarningForTwoPublicConstructors()
        {
            var code = $@"
namespace Agoda.Analyzers.Test
{{
    public class TestClass
    {{
        private int value;

        public TestClass()
        {{
            value = 0;
        }}

        public TestClass(int value)
        {{
            this.value = value;
        }}
    }}
}}";

            var doc = CreateProject(new[] {code})
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] {doc}, CancellationToken.None)
                .ConfigureAwait(false);

            var baseResult = CSharpDiagnostic(AG0006ClassShouldNotHaveMoreThanOnePublicConstructor.DiagnosticId);
            VerifyDiagnosticResults(diag, analyzersArray, new[]
            {
                baseResult.WithLocation(4, 5)
            });
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0006ClassShouldNotHaveMoreThanOnePublicConstructor();
        }
    }
}