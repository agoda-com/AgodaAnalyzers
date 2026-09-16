using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
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
            // The placeholder is the last identifier of the member access; "{Ex.Message}" would be
            // an illegal Serilog property name (and is what Sonar's S6674 reports).
            ExpectedFix = "_logger.LogError(\"Error occurred: {Message}\", ex.Message);",
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
            ExpectedFix = "_logger.Information(\"Received request from {Ip} at {Now}\", ip, DateTime.Now);",
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

        // ---------------------------------------------------------------------------------------
        // Overloads where the template is not the first argument.
        // ---------------------------------------------------------------------------------------

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Microsoft.Extensions.Logging;",
            SetupCode = "private readonly ILogger _logger;private Exception ex;",
            LogStatement = "_logger.LogError(ex, $\"Failed to load {name}\");",
            ExpectedFix = "_logger.LogError(ex, \"Failed to load {Name}\", name);",
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(AG0041LogTemplateAnalyzer.Rule)
                    .WithSpan(13, 34, 13, 58)
                    .WithArguments("string interpolation")
            }
        }).SetName("ILogger exception-first overload - exception argument is preserved");

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Microsoft.Extensions.Logging;",
            SetupCode = "private readonly ILogger _logger;",
            LogStatement = "_logger.LogInformation(new EventId(1, \"start\"), $\"User {name} arrived\");",
            ExpectedFix = "_logger.LogInformation(new EventId(1, \"start\"), \"User {Name} arrived\", name);",
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(AG0041LogTemplateAnalyzer.Rule)
                    .WithSpan(13, 61, 13, 83)
                    .WithArguments("string interpolation")
            }
        }).SetName("ILogger EventId-first overload");

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Microsoft.Extensions.Logging;",
            SetupCode = "private readonly ILogger _logger;private Exception ex;",
            LogStatement = "_logger.LogError(ex, \"Failed for {Name}\", name);",
            ExpectedFix = "_logger.LogError(ex, \"Failed for {Name}\", name);",
            ExpectedDiagnostics = new DiagnosticResult[] { }
        }).SetName("ILogger exception-first overload with a correct template (no diagnostics)");

        // ---------------------------------------------------------------------------------------
        // Receivers other than a field.
        // ---------------------------------------------------------------------------------------

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Serilog;",
            SetupCode = "private int elapsed;",
            LogStatement = "Log.Information($\"Finished in {elapsed}ms\");",
            ExpectedFix = "Log.Information(\"Finished in {Elapsed}ms\", elapsed);",
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(AG0041LogTemplateAnalyzer.Rule)
                    .WithSpan(13, 29, 13, 55)
                    .WithArguments("string interpolation")
            }
        }).SetName("Static Serilog Log entry point");

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Serilog;",
            SetupCode = "private Exception exception;",
            LogStatement = "Log.Error(exception, $\"Something went wrong with {nameof(TestMethod)}\");",
            ExpectedFix = "Log.Error(exception, \"Something went wrong with {TestMethod}\", nameof(TestMethod));",
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(AG0041LogTemplateAnalyzer.Rule)
                    .WithSpan(13, 34, 13, 83)
                    .WithArguments("string interpolation")
            }
        }).SetName("Static Serilog Log entry point - exception first and nameof placeholder");

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Microsoft.Extensions.Logging;",
            SetupCode = "private ILogger Logger { get; set; }",
            LogStatement = "Logger.LogInformation($\"Property {name}\");",
            ExpectedFix = "Logger.LogInformation(\"Property {Name}\", name);",
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(AG0041LogTemplateAnalyzer.Rule)
                    .WithSpan(13, 35, 13, 53)
                    .WithArguments("string interpolation")
            }
        }).SetName("Logger exposed as a property");

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Microsoft.Extensions.Logging;",
            SetupCode = "private readonly ILogger _logger;",
            LogStatement = "var log = _logger;\n            log.LogInformation($\"Local {name}\");",
            ExpectedFix = "var log = _logger;\n            log.LogInformation(\"Local {Name}\", name);",
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(AG0041LogTemplateAnalyzer.Rule)
                    .WithSpan(14, 32, 14, 47)
                    .WithArguments("string interpolation")
            }
        }).SetName("Logger held in a local");

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Microsoft.Extensions.Logging;",
            SetupCode = "private readonly ILogger _logger;",
            LogStatement = "void Helper(ILogger logger) { logger.LogInformation($\"Param {name}\"); }",
            ExpectedFix = "void Helper(ILogger logger) { logger.LogInformation(\"Param {Name}\", name); }",
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(AG0041LogTemplateAnalyzer.Rule)
                    .WithSpan(13, 65, 13, 80)
                    .WithArguments("string interpolation")
            }
        }).SetName("Logger passed as a parameter");

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Serilog;",
            SetupCode = "private class Foo { public static void Information(string s) {} }",
            LogStatement = "Foo.Information($\"hello {name}\");",
            ExpectedFix = "Foo.Information($\"hello {name}\");",
            ExpectedDiagnostics = new DiagnosticResult[] { }
        }).SetName("Non-logger type with a matching member name (no diagnostics)");

        // ---------------------------------------------------------------------------------------
        // Placeholder naming.
        // ---------------------------------------------------------------------------------------

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Microsoft.Extensions.Logging;",
            SetupCode = "private readonly ILogger _logger;private System.Diagnostics.Stopwatch stopwatch;",
            LogStatement = "_logger.LogInformation($\"Took {stopwatch.ElapsedMilliseconds}ms\");",
            ExpectedFix = "_logger.LogInformation(\"Took {ElapsedMilliseconds}ms\", stopwatch.ElapsedMilliseconds);",
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(AG0041LogTemplateAnalyzer.Rule)
                    .WithSpan(13, 36, 13, 77)
                    .WithArguments("string interpolation")
            }
        }).SetName("Member access placeholder uses the last identifier");

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Microsoft.Extensions.Logging;",
            SetupCode = "private readonly ILogger _logger;private DateTime fromDate;",
            LogStatement = "_logger.LogInformation($\"From {fromDate.ToShortDateString()}\");",
            ExpectedFix = "_logger.LogInformation(\"From {FromDate}\", fromDate.ToShortDateString());",
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(AG0041LogTemplateAnalyzer.Rule)
                    .WithSpan(13, 36, 13, 74)
                    .WithArguments("string interpolation")
            }
        }).SetName("Invocation placeholder uses the receiver name");

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Microsoft.Extensions.Logging;",
            SetupCode = "private readonly ILogger _logger;private Exception a;private Exception b;",
            LogStatement = "_logger.LogWarning($\"{a.Message} vs {b.Message}\");",
            ExpectedFix = "_logger.LogWarning(\"{Message} vs {Message2}\", a.Message, b.Message);",
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(AG0041LogTemplateAnalyzer.Rule)
                    .WithSpan(13, 32, 13, 61)
                    .WithArguments("string interpolation")
            }
        }).SetName("Colliding placeholder names get a numeric suffix");

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Microsoft.Extensions.Logging;",
            SetupCode = "private readonly ILogger _logger;",
            LogStatement = "_logger.LogInformation($\"Next {age + 1} and {name.Length > 3}\");",
            ExpectedFix = "_logger.LogInformation(\"Next {Arg1} and {Arg2}\", age + 1, name.Length > 3);",
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(AG0041LogTemplateAnalyzer.Rule)
                    .WithSpan(13, 36, 13, 75)
                    .WithArguments("string interpolation")
            }
        }).SetName("Unnamed expressions fall back to ArgN");

        // ---------------------------------------------------------------------------------------
        // Format specifiers, alignment, escaping and literal flavours.
        // ---------------------------------------------------------------------------------------

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Microsoft.Extensions.Logging;",
            SetupCode = "private readonly ILogger _logger;private decimal price;",
            LogStatement = "_logger.LogInformation($\"Price: {price:F2}\");",
            ExpectedFix = "_logger.LogInformation(\"Price: {Price:F2}\", price);",
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(AG0041LogTemplateAnalyzer.Rule)
                    .WithSpan(13, 36, 13, 56)
                    .WithArguments("string interpolation")
            }
        }).SetName("Format specifier is carried over");

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Microsoft.Extensions.Logging;",
            SetupCode = "private readonly ILogger _logger;",
            LogStatement = "_logger.LogInformation($\"Age {age,10}!\");",
            ExpectedFix = "_logger.LogInformation(\"Age {Age,10}!\", age);",
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(AG0041LogTemplateAnalyzer.Rule)
                    .WithSpan(13, 36, 13, 52)
                    .WithArguments("string interpolation")
            }
        }).SetName("Alignment clause is carried over");

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Microsoft.Extensions.Logging;",
            SetupCode = "private readonly ILogger _logger;",
            LogStatement = "_logger.LogInformation($\"Set {{literal}} for {name}\");",
            ExpectedFix = "_logger.LogInformation(\"Set {{literal}} for {Name}\", name);",
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(AG0041LogTemplateAnalyzer.Rule)
                    .WithSpan(13, 36, 13, 65)
                    .WithArguments("string interpolation")
            }
        }).SetName("Escaped braces stay escaped in the template");

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Serilog;",
            SetupCode = "private readonly ILogger _logger;",
            LogStatement = "_logger.Debug(\"Braces {x}: \" + name);",
            ExpectedFix = "_logger.Debug(\"Braces {{x}}: {Name}\", name);",
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(AG0041LogTemplateAnalyzer.Rule)
                    .WithSpan(13, 27, 13, 48)
                    .WithArguments("string concatenation")
            }
        }).SetName("Braces in a concatenated literal are escaped");

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Microsoft.Extensions.Logging;",
            SetupCode = "private readonly ILogger _logger;",
            LogStatement = "_logger.LogInformation($@\"He said \"\"{name}\"\"\");",
            ExpectedFix = "_logger.LogInformation(\"He said \\\"{Name}\\\"\", name);",
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(AG0041LogTemplateAnalyzer.Rule)
                    .WithSpan(13, 36, 13, 58)
                    .WithArguments("string interpolation")
            }
        }).SetName("Verbatim interpolated string becomes a normal escaped literal");

        yield return new TestCaseData(new TestCase
        {
            Usings = "using Microsoft.Extensions.Logging;",
            SetupCode = "private readonly ILogger _logger;",
            LogStatement = "_logger.LogInformation($\"\"\"Raw {name}\"\"\");",
            ExpectedFix = "_logger.LogInformation(\"Raw {Name}\", name);",
            ExpectedDiagnostics = new[]
            {
                new DiagnosticResult(AG0041LogTemplateAnalyzer.Rule)
                    .WithSpan(13, 36, 13, 53)
                    .WithArguments("string interpolation")
            }
        }).SetName("Raw interpolated string becomes a normal literal");
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

    /// <summary>
    /// The fix is also offered for Sonar's S2629, which reports the same problem. Sonar itself is
    /// not referenced here - a stub analyzer reports the ID so that we test our own wiring rather
    /// than Sonar's detection.
    /// </summary>
    [Test]
    public async Task FixAppliesToSonarS2629()
    {
        const string test = @"
using System;
using Microsoft.Extensions.Logging;

namespace TestNamespace
{
    public class TestClass
    {
        private readonly ILogger _logger;
        private Exception ex;

        public void TestMethod(string name)
        {
            _logger.LogError(ex, $""Sonar reported {name}"");
        }
    }
}";

        const string expected = @"
using System;
using Microsoft.Extensions.Logging;

namespace TestNamespace
{
    public class TestClass
    {
        private readonly ILogger _logger;
        private Exception ex;

        public void TestMethod(string name)
        {
            _logger.LogError(ex, ""Sonar reported {Name}"", name);
        }
    }
}";

        var codeFixTest = new SonarCodeFixTest(test, expected, new[]
        {
            new DiagnosticResult(StubSonarS2629Analyzer.Rule).WithSpan(14, 34, 14, 58)
        });

        await codeFixTest.RunAsync(CancellationToken.None);
    }

    private static ReferenceAssemblies LoggingReferenceAssemblies => ReferenceAssemblies.Default
        .AddPackages(ImmutableArray.Create(
            new PackageIdentity("Microsoft.Extensions.Logging.Abstractions", "6.0.0"),
            new PackageIdentity("Serilog", "2.10.0")
        ));

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

            ReferenceAssemblies = LoggingReferenceAssemblies;
        }
    }

    private class SonarCodeFixTest : CSharpCodeFixTest<StubSonarS2629Analyzer, AG0041CodeFixProvider, NUnitVerifier>
    {
        public SonarCodeFixTest(
            string source,
            string fixedSource,
            IEnumerable<DiagnosticResult> expectedDiagnostics)
        {
            TestCode = source;
            FixedCode = fixedSource;
            ExpectedDiagnostics.AddRange(expectedDiagnostics);

            ReferenceAssemblies = LoggingReferenceAssemblies;
        }
    }

    /// <summary>
    /// Stands in for SonarAnalyzer.CSharp's S2629 so the test suite does not need a dependency on
    /// it. It reports the same shape: an interpolated string passed to a member invocation.
    /// </summary>
    /// <remarks>
    /// The rules that police shipped analyzer projects do not apply to a stub that never leaves the
    /// test assembly, and its title/message deliberately mirror Sonar's wording verbatim:
    ///   RS1036 - EnforceExtendedAnalyzerRules is for projects that package analyzers; this one packages tests.
    ///   RS2008 - S2629 is owned by SonarAnalyzer.CSharp, so it is not tracked in our release files.
    ///   RS1031/RS1032 - the trailing period comes from Sonar's own text, which we copy on purpose.
    /// </remarks>
#pragma warning disable RS1036
#pragma warning disable RS2008
#pragma warning disable RS1031
#pragma warning disable RS1032
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StubSonarS2629Analyzer : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            "S2629",
            "Don't use string interpolation in logging message templates.",
            "Don't use string interpolation in logging message templates.",
            "Major Code Smell",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            if (!(invocation.Expression is MemberAccessExpressionSyntax))
                return;

            var argument = invocation.ArgumentList.Arguments
                .FirstOrDefault(a => a.Expression is InterpolatedStringExpressionSyntax);
            if (argument == null)
                return;

            context.ReportDiagnostic(Diagnostic.Create(Rule, argument.GetLocation()));
        }
    }
#pragma warning restore RS1032
#pragma warning restore RS1031
#pragma warning restore RS2008
#pragma warning restore RS1036

    public class TestCase
    {
        public string Usings { get; set; }
        public string SetupCode { get; set; }
        public string LogStatement { get; set; }
        public string ExpectedFix { get; set; }
        public DiagnosticResult[] ExpectedDiagnostics { get; set; }
    }
}
