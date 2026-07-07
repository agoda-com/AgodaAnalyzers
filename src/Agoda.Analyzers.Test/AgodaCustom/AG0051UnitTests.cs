using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom;

[TestFixture]
internal class AG0051UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0051DetectHardcodedDateLiterals();

    protected override string DiagnosticId => AG0051DetectHardcodedDateLiterals.DiagnosticId;

    private const int FutureYear = 2899;

    private static string WrapInTestNamespace(string body) =>
        @"
using System;
namespace MyApp.Tests {" + body + @"
}
";

    [Test]
    public async Task AG0051_NewDateTimeWithFutureHardcodedDate_ShouldShowWarning()
    {
        var code = WrapInTestNamespace($@"
class TestClass
{{
    public void TestMethod()
    {{
        var date = new DateTime({FutureYear}, 12, 31);
    }}
}}");
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(8, 20));
    }

    [Test]
    public async Task AG0051_NewDateTimeOffsetWithHardcodedDate_ShouldShowWarning()
    {
        var code = WrapInTestNamespace($@"
class TestClass
{{
    public void TestMethod()
    {{
        var date = new DateTimeOffset({FutureYear}, 8, 15, 12, 0, 0, TimeSpan.Zero);
    }}
}}");
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(8, 20));
    }

    [Test]
    public async Task AG0051_DateTimeParseWithHardcodedString_ShouldShowWarning()
    {
        var code = WrapInTestNamespace($@"
class TestClass
{{
    public void TestMethod()
    {{
        var date = DateTime.Parse(""{FutureYear}-12-31"");
    }}
}}");
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(8, 20));
    }

    [Test]
    public async Task AG0051_DateTimeOffsetParseWithHardcodedString_ShouldShowWarning()
    {
        var code = WrapInTestNamespace($@"
class TestClass
{{
    public void TestMethod()
    {{
        var date = DateTimeOffset.Parse(""{FutureYear}-12-31T00:00:00Z"");
    }}
}}");
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(8, 20));
    }

    [Test]
    public async Task AG0051_DateTimeWithFarPastDate_ShouldNotShowWarning()
    {
        var code = WrapInTestNamespace(@"
class TestClass
{
    public void TestMethod()
    {
        var date = new DateTime(2000, 1, 1);
    }
}");
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0051_DateTimeParseWithFarPastDate_ShouldNotShowWarning()
    {
        var code = WrapInTestNamespace(@"
class TestClass
{
    public void TestMethod()
    {
        var date = DateTime.Parse(""2010-06-15"");
    }
}");
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0051_DateTimeTodayWithOffset_ShouldNotShowWarning()
    {
        var code = WrapInTestNamespace(@"
class TestClass
{
    public void TestMethod()
    {
        var date = DateTime.Today.AddDays(30);
    }
}");
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0051_DateTimeOffsetUtcNowWithOffset_ShouldNotShowWarning()
    {
        var code = WrapInTestNamespace(@"
class TestClass
{
    public void TestMethod()
    {
        var date = DateTimeOffset.UtcNow.AddDays(30);
    }
}");
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0051_NewDateTimeWithVariableArgs_ShouldNotShowWarning()
    {
        var code = WrapInTestNamespace($@"
class TestClass
{{
    public void TestMethod()
    {{
        int year = {FutureYear};
        var date = new DateTime(year, 12, 31);
    }}
}}");
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0051_MultipleDateTimeLiterals_ShouldShowMultipleWarnings()
    {
        var code = WrapInTestNamespace($@"
class TestClass
{{
    public void TestMethod()
    {{
        var checkIn = new DateTime({FutureYear}, 12, 31);
        var checkOut = new DateTime({FutureYear + 1}, 1, 5);
    }}
}}");
        await VerifyDiagnosticsAsync(code, new[]
        {
            new DiagnosticLocation(8, 23),
            new DiagnosticLocation(9, 24)
        });
    }

    [Test]
    public async Task AG0051_DateTimeParseWithNonDateString_ShouldNotShowWarning()
    {
        var code = WrapInTestNamespace(@"
class TestClass
{
    public void TestMethod()
    {
        var parsed = int.Parse(""42"");
    }
}");
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0051_NewDateTimeWithPast2020Date_ShouldNotShowWarning()
    {
        var code = WrapInTestNamespace(@"
class TestClass
{
    public void TestMethod()
    {
        var date = new DateTime(2020, 6, 15);
    }
}");
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0051_NewDateTimeWith2019Date_ShouldNotShowWarning()
    {
        var code = WrapInTestNamespace(@"
class TestClass
{
    public void TestMethod()
    {
        var date = new DateTime(2019, 12, 31);
    }
}");
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0051_DateTimeParseWithSlashFormat_ShouldShowWarning()
    {
        var code = WrapInTestNamespace($@"
class TestClass
{{
    public void TestMethod()
    {{
        var date = DateTime.Parse(""{FutureYear}/06/15"");
    }}
}}");
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(8, 20));
    }

    [Test]
    public async Task AG0051_FieldInitializerWithHardcodedDate_ShouldShowWarning()
    {
        var code = WrapInTestNamespace($@"
class TestClass
{{
    private readonly DateTime _endDate = new DateTime({FutureYear}, 3, 31);

    public void TestMethod()
    {{
    }}
}}");
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(6, 42));
    }

    [Test]
    public async Task AG0051_NonTestNamespace_ShouldNotShowWarning()
    {
        var code = $@"
using System;

namespace MyApp.Services
{{
    class DateService
    {{
        public void SetDate()
        {{
            var date = new DateTime({FutureYear}, 12, 31);
        }}
    }}
}}
";
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0051_TestsNamespaceVariant_ShouldShowWarning()
    {
        var code = $@"
using System;

namespace MyApp.IntegrationTests.Booking
{{
    class BookingTests
    {{
        public void TestMethod()
        {{
            var date = new DateTime({FutureYear}, 12, 31);
        }}
    }}
}}
";
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(10, 24));
    }

    [Test]
    public async Task AG0051_DateTimeParseWithPast2020Date_ShouldNotShowWarning()
    {
        var code = WrapInTestNamespace(@"
class TestClass
{
    public void TestMethod()
    {
        var date = DateTime.Parse(""2020-06-15"");
    }
}");
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0051_NewDateTimeWithSentinelYear_ShouldNotShowWarning()
    {
        var code = WrapInTestNamespace(@"
class TestClass
{
    public void TestMethod()
    {
        var date = new DateTime(9999, 12, 31);
    }
}");
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0051_DateTimeParseWithSentinelYear_ShouldNotShowWarning()
    {
        var code = WrapInTestNamespace(@"
class TestClass
{
    public void TestMethod()
    {
        var date = DateTime.Parse(""2999-12-31"");
    }
}");
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0051_NewDateTimeAssignedToLowRiskObjectInitializerProperty_ShouldNotShowWarning()
    {
        var code = WrapInTestNamespace($@"
class TestClass
{{
    class ExpectedDto {{ public DateTime DisplayDate {{ get; set; }} }}

    public void TestMethod()
    {{
        var expected = new ExpectedDto {{ DisplayDate = new DateTime({FutureYear}, 12, 31) }};
    }}
}}");
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0051_NewDateTimeAssignedToRiskyObjectInitializerProperty_ShouldShowWarning()
    {
        var code = WrapInTestNamespace($@"
class TestClass
{{
    class Offer {{ public DateTime StartDate {{ get; set; }} }}

    public void TestMethod()
    {{
        var offer = new Offer {{ StartDate = new DateTime({FutureYear}, 12, 31) }};
    }}
}}");
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(10, 45));
    }

    [Test]
    public async Task AG0051_NewDateTimeUsedAsAssertionArgument_ShouldNotShowWarning()
    {
        var code = WrapInTestNamespace($@"
static class AssertionExtensions
{{
    public static void ShouldBe<T>(this T actual, T expected)
    {{
    }}
}}

class TestClass
{{
    public void TestMethod()
    {{
        var actual = DateTime.Today;
        actual.ShouldBe(new DateTime({FutureYear}, 12, 31));
    }}
}}");
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0051_FutureHardcodedDateDiagnostic_ShouldIncludeHighConfidence()
    {
        var code = WrapInTestNamespace($@"
class TestClass
{{
    public void TestMethod()
    {{
        var date = new DateTime({FutureYear}, 12, 31);
    }}
}}");

        var document = CreateProject(new[] { code }).Documents.First();
        var diagnostics = await GetSortedDiagnosticsFromDocumentsAsync(
            ImmutableArray.Create(DiagnosticAnalyzer),
            new[] { document },
            CancellationToken.None);

        Assert.AreEqual(1, diagnostics.Length);
        Assert.IsTrue(diagnostics[0].Properties.TryGetValue(
            AG0051DetectHardcodedDateLiterals.ConfidencePropertyName,
            out var confidence));
        Assert.AreEqual(AG0051DetectHardcodedDateLiterals.HighConfidence, confidence);
    }
}
