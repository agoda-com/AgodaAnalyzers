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

    [Test]
    public async Task AG0051_NewDateTimeWithRecentHardcodedDate_ShouldShowWarning()
    {
        var code = @"
using System;

class TestClass
{
    public void TestMethod()
    {
        var date = new DateTime(2025, 12, 31);
    }
}
";
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(8, 20));
    }

    [Test]
    public async Task AG0051_NewDateTimeOffsetWithHardcodedDate_ShouldShowWarning()
    {
        var code = @"
using System;

class TestClass
{
    public void TestMethod()
    {
        var date = new DateTimeOffset(2025, 8, 15, 12, 0, 0, TimeSpan.Zero);
    }
}
";
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(8, 20));
    }

    [Test]
    public async Task AG0051_DateTimeParseWithHardcodedString_ShouldShowWarning()
    {
        var code = @"
using System;

class TestClass
{
    public void TestMethod()
    {
        var date = DateTime.Parse(""2025-12-31"");
    }
}
";
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(8, 20));
    }

    [Test]
    public async Task AG0051_DateTimeOffsetParseWithHardcodedString_ShouldShowWarning()
    {
        var code = @"
using System;

class TestClass
{
    public void TestMethod()
    {
        var date = DateTimeOffset.Parse(""2025-12-31T00:00:00Z"");
    }
}
";
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(8, 20));
    }

    [Test]
    public async Task AG0051_DateTimeWithFarPastDate_ShouldNotShowWarning()
    {
        var code = @"
using System;

class TestClass
{
    public void TestMethod()
    {
        var date = new DateTime(2000, 1, 1);
    }
}
";
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0051_DateTimeParseWithFarPastDate_ShouldNotShowWarning()
    {
        var code = @"
using System;

class TestClass
{
    public void TestMethod()
    {
        var date = DateTime.Parse(""2010-06-15"");
    }
}
";
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0051_DateTimeTodayWithOffset_ShouldNotShowWarning()
    {
        var code = @"
using System;

class TestClass
{
    public void TestMethod()
    {
        var date = DateTime.Today.AddDays(30);
    }
}
";
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0051_DateTimeOffsetUtcNowWithOffset_ShouldNotShowWarning()
    {
        var code = @"
using System;

class TestClass
{
    public void TestMethod()
    {
        var date = DateTimeOffset.UtcNow.AddDays(30);
    }
}
";
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0051_NewDateTimeWithVariableArgs_ShouldNotShowWarning()
    {
        var code = @"
using System;

class TestClass
{
    public void TestMethod()
    {
        int year = 2025;
        var date = new DateTime(year, 12, 31);
    }
}
";
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0051_MultipleDateTimeLiterals_ShouldShowMultipleWarnings()
    {
        var code = @"
using System;

class TestClass
{
    public void TestMethod()
    {
        var checkIn = new DateTime(2025, 12, 31);
        var checkOut = new DateTime(2026, 1, 5);
    }
}
";
        await VerifyDiagnosticsAsync(code, new[]
        {
            new DiagnosticLocation(8, 23),
            new DiagnosticLocation(9, 24)
        });
    }

    [Test]
    public async Task AG0051_DateTimeParseWithNonDateString_ShouldNotShowWarning()
    {
        var code = @"
using System;

class TestClass
{
    public void TestMethod()
    {
        var parsed = int.Parse(""42"");
    }
}
";
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0051_NewDateTimeWith2020Date_ShouldShowWarning()
    {
        var code = @"
using System;

class TestClass
{
    public void TestMethod()
    {
        var date = new DateTime(2020, 6, 15);
    }
}
";
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(8, 20));
    }

    [Test]
    public async Task AG0051_NewDateTimeWith2019Date_ShouldNotShowWarning()
    {
        var code = @"
using System;

class TestClass
{
    public void TestMethod()
    {
        var date = new DateTime(2019, 12, 31);
    }
}
";
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0051_DateTimeParseWithSlashFormat_ShouldShowWarning()
    {
        var code = @"
using System;

class TestClass
{
    public void TestMethod()
    {
        var date = DateTime.Parse(""2025/06/15"");
    }
}
";
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(8, 20));
    }

    [Test]
    public async Task AG0051_FieldInitializerWithHardcodedDate_ShouldShowWarning()
    {
        var code = @"
using System;

class TestClass
{
    private readonly DateTime _endDate = new DateTime(2025, 3, 31);

    public void TestMethod()
    {
    }
}
";
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(6, 42));
    }
}
