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
    internal class AG0008UnitTests : DiagnosticVerifier
    {
        [Test]
        public async Task PreventReturningNullForReturnValueOfIEnumerable_ShouldReportCorrectly()
        {
            var code = @"
using System.Collections.Generic;

namespace Agoda.Analyzers.Test
{
    public class TestClass
    {
        public IEnumerable<string> GetValuesForId(int id)
        {
            return null;
        }
    }
}";

            var doc = CreateProject(new[] {code})
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] {doc}, CancellationToken.None)
                .ConfigureAwait(false);

            var expected = CSharpDiagnostic(AG0008PreventReturningNullForReturnValueOfIEnumerable.DiagnosticId);
            VerifyDiagnosticResults(diag, analyzersArray, new DiagnosticResult[] {
                expected.WithLocation(10, 13)
            });
        }

        [Test]
        public async Task PreventReturningNullForReturnValueOfIEnumerable_ShouldReportCorrectlyForClassImplementingIEnumerable()
        {
            var code = @"
using System.Collections.Generic;

namespace Agoda.Analyzers.Test
{
    public class TestClass
    {
        public List<string> GetValuesForId(int id)
        {
            return null;
        }
    }
}";

            var doc = CreateProject(new[] {code})
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] {doc}, CancellationToken.None)
                .ConfigureAwait(false);

            var expected = CSharpDiagnostic(AG0008PreventReturningNullForReturnValueOfIEnumerable.DiagnosticId);
            VerifyDiagnosticResults(diag, analyzersArray, new DiagnosticResult[] {
                expected.WithLocation(10, 13)
            });
        }

        [Test]
        public async Task PreventReturningNullForReturnValueOfIEnumerable_ShouldNotReportStringReturnType()
        {
            var code = @"
using System.Collections.Generic;

namespace Agoda.Analyzers.Test
{
    public class TestClass
    {
        public string GetValuesForId(int id)
        {
            return null;
        }
    }
}";

            var doc = CreateProject(new[] {code})
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] {doc}, CancellationToken.None)
                .ConfigureAwait(false);

            VerifyDiagnosticResults(diag, analyzersArray, new DiagnosticResult[]{ });
        }

        [Test]
        public async Task PreventReturningNullForReturnValueOfIEnumerable_ShouldNotReportNullableInt()
        {
            var code = @"
using System.Collections.Generic;

namespace Agoda.Analyzers.Test
{
    public class TestClass
    {
        public int? GetValuesForId(int id)
        {
            return null;
        }
    }
}";

            var doc = CreateProject(new[] { code })
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] { doc }, CancellationToken.None)
                .ConfigureAwait(false);

            VerifyDiagnosticResults(diag, analyzersArray, new DiagnosticResult[] { });
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0008PreventReturningNullForReturnValueOfIEnumerable();
        }
    }
}