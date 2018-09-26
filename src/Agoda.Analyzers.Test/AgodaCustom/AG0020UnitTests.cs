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
    internal class AG0020UnitTests : DiagnosticVerifier
    {
        protected DiagnosticAnalyzer DiagnosticAnalyzer => new AG0020AvoidReturningNullEnumerables();
        
        protected string DiagnosticId => AG0020AvoidReturningNullEnumerables.DIAGNOSTIC_ID;

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

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(10, 20), DiagnosticId, DiagnosticAnalyzer);
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

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(10, 20), DiagnosticId, DiagnosticAnalyzer);
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

            var doc = CreateProject(new[] {code}, DiagnosticAnalyzer)
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers(DiagnosticAnalyzer).ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] {doc}, CancellationToken.None)
                .ConfigureAwait(false);

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults, DiagnosticId, DiagnosticAnalyzer);
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

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults, DiagnosticId, DiagnosticAnalyzer);
        }

        [Test]
        public async Task PreventReturningNullForReturnValueOfIEnumerable_ShouldReportNullReturnFromProperty()
        {
            var code = @"
using System.Collections.Generic;

namespace Agoda.Analyzers.Test
{
    public class TestClass
    {
        public IEnumerable<string> Data {
            get {
                return null;
            }
        }
        public IEnumerable<string> SecondProperty => null;
    }
}";

            await VerifyDiagnosticsAsync(code, new[] {
                new DiagnosticLocation(10, 24),
                new DiagnosticLocation(13, 54)
            }, DiagnosticId, DiagnosticAnalyzer);
        }

        [Test]
        public async Task PreventReturningNullForReturnValueOfIEnumerable_ShouldNotReportNullReturnFromPropertyOfStringOrOtherType()
        {
            var code = @"
using System.Collections.Generic;

namespace Agoda.Analyzers.Test
{
    public class TestClass
    {
        public string ThisIsOk => null;
        public int? SomeOptionalNumber => null;
    }
}";

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults, DiagnosticId, DiagnosticAnalyzer);
        }

        [Test]
        public async Task PreventReturningNullForReturnValueOfIEnumerable_ShouldReportNullReturnFromStringArray()
        {
            var code = @"
namespace Agoda.Analyzers.Test
{
    public class TestClass
    {
        public string[] Method()
        {
            return null;
        }
    }
}";

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(8, 20), DiagnosticId, DiagnosticAnalyzer);
        }

        
    }
}