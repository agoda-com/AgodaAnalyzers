using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom;

public class AG0041LogTemplateAnalyzerTests
{
    private static IEnumerable<TestCaseData> TestCases()
    {
        yield return new TestCaseData(new TestCase
        {
            Usings = "using Microsoft.Extensions.Logging;",
            SetupCode = "private readonly ILogger _logger;",
            LogStatement = "_logger.LogInformation($\"User {name} is {age} years old\");",
            ExpectedFix = "_logger.LogInformation(\"User {Name} is {Age} years old\", name, age);",
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(AG0041LogTemplateAnalyzer.Rule)
                    .WithSpan(13, 36, 13, 69)
                    .WithArguments("string interpolation")
            }
        }).SetName("ILogger with string interpolation");

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Serilog;",
            SetupCode = "private readonly ILogger _logger;",
            LogStatement = "_logger.Information(\"User \" + name + \" is \" + age + \" years old\");",
            ExpectedFix = "_logger.Information(\"User {Name} is {Age} years old\", name, age);",
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(AG0041LogTemplateAnalyzer.Rule)
                    .WithSpan(13, 33, 13, 77)
                    .WithArguments("string concatenation")
            }
        }).SetName("Serilog with string concatenation");

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Microsoft.Extensions.Logging;",
            SetupCode = "private readonly ILogger _logger;",
            LogStatement = "_logger.LogWarning(\"User {Name} is {Age} years old\", name, age);",
            ExpectedFix = "_logger.LogWarning(\"User {Name} is {Age} years old\", name, age);",
            ExpectedDiagnostics = new DiagnosticResult[] { }
        }).SetName("ILogger with correct usage (no diagnostics)");

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Microsoft.Extensions.Logging;",
            SetupCode = "private readonly ILogger _logger;private Exception ex;",
            LogStatement = "_logger.LogError($\"Error occurred: {ex.Message}\");",
            ExpectedFix = "_logger.LogError(\"Error occurred: {Ex.Message}\", ex.Message);",
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(AG0041LogTemplateAnalyzer.Rule)
                    .WithSpan(13, 30, 13, 61)
                    .WithArguments("string interpolation")
            }
        }).SetName("ILogger with string interpolation - error logging");

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Serilog;",
            SetupCode = "private readonly ILogger _logger;private string debugInfo;",
            LogStatement = "_logger.Debug(\"Debug info: \" + debugInfo);",
            ExpectedFix = "_logger.Debug(\"Debug info: {DebugInfo}\", debugInfo);",
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(AG0041LogTemplateAnalyzer.Rule)
                    .WithSpan(13, 27, 13, 53)
                    .WithArguments("string concatenation")
            }
        }).SetName("Serilog with string concatenation - debug logging");

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Microsoft.Extensions.Logging;",
            SetupCode = "private readonly ILogger _logger;private int itemId;private int userId;",
            LogStatement = "_logger.LogInformation(\"Processing item {ItemId} for user {UserId}\", itemId, userId);",
            ExpectedFix = "_logger.LogInformation(\"Processing item {ItemId} for user {UserId}\", itemId, userId);",
            ExpectedDiagnostics = new DiagnosticResult[] { }
        }).SetName("ILogger with correct usage - multiple parameters");

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Serilog;",
            SetupCode = "private readonly ILogger _logger;private string ip;",
            LogStatement = "_logger.Information($\"Received request from {ip} at {DateTime.Now}\");",
            ExpectedFix = "_logger.Information(\"Received request from {Ip} at {DateTime.Now}\", ip, DateTime.Now);",
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(AG0041LogTemplateAnalyzer.Rule)
                    .WithSpan(13, 33, 13, 80)
                    .WithArguments("string interpolation")
            }
        }).SetName("Serilog with string interpolation - complex expression");

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Microsoft.Extensions.Logging;\nusing Serilog;",
            SetupCode =
                "private readonly Microsoft.Extensions.Logging.ILogger _msLogger;\nprivate readonly Serilog.ILogger _seriLogger;private string info;",
            LogStatement =
                "_msLogger.LogInformation($\"MS: {info}\");\n            _seriLogger.Information(\"Seri: \" + info);",
            ExpectedFix =
                "_msLogger.LogInformation(\"MS: {Info}\", info);\n            _seriLogger.Information(\"Seri: {Info}\", info);",
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(AG0041LogTemplateAnalyzer.Rule)
                    .WithSpan(15, 38, 15, 51)
                    .WithArguments("string interpolation"),
                new DiagnosticResult(AG0041LogTemplateAnalyzer.Rule)
                    .WithSpan(16, 37, 16, 52)
                    .WithArguments("string concatenation")
            }
        }).SetName("Multiple loggers - both with issues");

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Microsoft.Extensions.Logging;",
            SetupCode = "private readonly ILogger _logger;private string username;private string ipAddress;",
            LogStatement =
                "_logger.LogInformation(\"User {Username} logged in from {IpAddress}\", username, ipAddress);",
            ExpectedFix =
                "_logger.LogInformation(\"User {Username} logged in from {IpAddress}\", username, ipAddress);",
            ExpectedDiagnostics = new DiagnosticResult[] { }
        }).SetName("ILogger with correct usage - multiple parameters with PascalCase");

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Serilog;",
            SetupCode = "private readonly ILogger _logger;",
            LogStatement = "_logger.Information(\"Status: {@Status}\", new { Code = 200, Message = \"OK\" });",
            ExpectedFix = "_logger.Information(\"Status: {@Status}\", new { Code = 200, Message = \"OK\" });",
            ExpectedDiagnostics = new DiagnosticResult[] { }
        }).SetName("Serilog with correct usage - complex object logging");
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public async Task TestLogTemplateUsage(TestCase testCase)
    {
        var test = $@"
using System;
{testCase.Usings}

namespace TestNamespace
{{
    public class TestClass
    {{
        {testCase.SetupCode}

        public void TestMethod(string name, int age)
        {{
            {testCase.LogStatement}
        }}
    }}
}}";

        var expected = $@"
using System;
{testCase.Usings}

namespace TestNamespace
{{
    public class TestClass
    {{
        {testCase.SetupCode}

        public void TestMethod(string name, int age)
        {{
            {testCase.ExpectedFix}
        }}
    }}
}}";

        var codeFixTest = new CodeFixTest(test, expected, testCase.ExpectedDiagnostics);

        await codeFixTest.RunAsync(CancellationToken.None);
    }

    private class CodeFixTest : CSharpCodeFixTest<AG0041LogTemplateAnalyzer, AG0041CodeFixProvider, NUnitVerifier>
    {
        public CodeFixTest(
            string source,
            string fixedSource,
            IEnumerable<DiagnosticResult> expectedDiagnostics)
        {
            TestCode = source;
            FixedCode = fixedSource;
            ExpectedDiagnostics.AddRange(expectedDiagnostics);

            ReferenceAssemblies = ReferenceAssemblies.Default
                .AddPackages(ImmutableArray.Create(
                    new PackageIdentity("Microsoft.Extensions.Logging.Abstractions", "6.0.0"),
                    new PackageIdentity("Serilog", "2.10.0")
                ));
        }
    }

    public class TestCase
    {
        public string Usings { get; set; }
        public string SetupCode { get; set; }
        public string LogStatement { get; set; }
        public string ExpectedFix { get; set; }
        public DiagnosticResult[] ExpectedDiagnostics { get; set; }
    }
}