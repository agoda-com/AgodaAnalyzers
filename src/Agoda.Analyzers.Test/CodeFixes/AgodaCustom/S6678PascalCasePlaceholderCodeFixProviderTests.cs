using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
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
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom;

/// <summary>
/// S6678 is owned by SonarAnalyzer.CSharp and we deliberately do not take a dependency on it - we are not
/// testing Sonar's detection, only our fix. The stub analyzers below report an "S6678" diagnostic on logging
/// message templates so the code fix has something to attach to.
/// </summary>
public class S6678PascalCasePlaceholderCodeFixProviderTests
{
    private const string SourceTemplate = @"
using System;
using Microsoft.Extensions.Logging;

namespace TestNamespace
{
    public class TestClass
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly Serilog.ILogger _seriLogger;

        public void TestMethod(string name, string x, int userId, decimal price, object propertyId,
            DateTime startDate, string service, string method, int roomTypeId, Exception ex, string message)
        {
            $$BODY$$
        }
    }
}";

    private static string Wrap(string body) => SourceTemplate.Replace("$$BODY$$", body);

    private static IEnumerable<TestCaseData> TestCases()
    {
        yield return Case(
            "Plain camelCase placeholder is PascalCased",
            @"_logger.LogInformation({|S6678:""User {name} logged in""|}, name);",
            @"_logger.LogInformation(""User {Name} logged in"", name);");

        yield return Case(
            "Destructuring prefix is preserved",
            @"_logger.LogInformation({|S6678:""Loaded {@propertyId}""|}, propertyId);",
            @"_logger.LogInformation(""Loaded {@PropertyId}"", propertyId);");

        yield return Case(
            "Stringification prefix is preserved",
            @"_logger.LogInformation({|S6678:""Value {$x}""|}, x);",
            @"_logger.LogInformation(""Value {$X}"", x);");

        yield return Case(
            "Alignment clause is preserved",
            @"_logger.LogInformation({|S6678:""User {userId,10} done""|}, userId);",
            @"_logger.LogInformation(""User {UserId,10} done"", userId);");

        yield return Case(
            "Format clause is preserved",
            @"_logger.LogInformation({|S6678:""Cost {price:F2}""|}, price);",
            @"_logger.LogInformation(""Cost {Price:F2}"", price);");

        yield return Case(
            "Alignment and format clauses are preserved together",
            @"_logger.LogInformation({|S6678:""Cost {price,-10:F2}""|}, price);",
            @"_logger.LogInformation(""Cost {Price,-10:F2}"", price);");

        yield return Case(
            "Positional placeholders are left alone",
            @"_logger.LogInformation({|S6678:""Slot {0} and {1,5:F2} for {userId}""|}, userId);",
            @"_logger.LogInformation(""Slot {0} and {1,5:F2} for {UserId}"", userId);");

        yield return Case(
            "Escaped braces are left alone",
            @"_logger.LogInformation({|S6678:""Braces {{name}} and {{0}} for {userId}""|}, userId);",
            @"_logger.LogInformation(""Braces {{name}} and {{0}} for {UserId}"", userId);");

        yield return Case(
            "Placeholder starting with underscore is left alone",
            @"_logger.LogInformation({|S6678:""Internal {_state} for {userId}""|}, userId);",
            @"_logger.LogInformation(""Internal {_state} for {UserId}"", userId);");

        yield return Case(
            "Already PascalCase reports nothing and changes nothing",
            @"_logger.LogInformation(""User {Name} is {UserId}"", name, userId);",
            @"_logger.LogInformation(""User {Name} is {UserId}"", name, userId);");

        yield return Case(
            "Template with only positional and escaped braces reports nothing",
            @"_logger.LogInformation(""Slot {0} with {{braces}}"", userId);",
            @"_logger.LogInformation(""Slot {0} with {{braces}}"", userId);");

        yield return Case(
            "Multiple placeholders in one template",
            @"_logger.LogInformation({|S6678:""{service}.{method} failed for {userId}""|}, service, method, userId);",
            @"_logger.LogInformation(""{Service}.{Method} failed for {UserId}"", service, method, userId);");

        yield return Case(
            "Other arguments and trivia survive unchanged",
            @"_logger.LogError(ex, {|S6678:""Failed for {userId} at {startDate:O}""|}, userId, /* keep me */ startDate);",
            @"_logger.LogError(ex, ""Failed for {UserId} at {StartDate:O}"", userId, /* keep me */ startDate);");

        yield return Case(
            "Serilog instance logger",
            @"_seriLogger.Information({|S6678:""Saved {roomTypeId}""|}, roomTypeId);",
            @"_seriLogger.Information(""Saved {RoomTypeId}"", roomTypeId);");

        yield return Case(
            "Verbatim literal keeps its form",
            @"_logger.LogInformation({|S6678:@""Path C:\temp for {userId}""|}, userId);",
            @"_logger.LogInformation(@""Path C:\temp for {UserId}"", userId);");

        yield return Case(
            "Raw string literal keeps its form",
            @"_logger.LogInformation({|S6678:""""""Raw {userId} value""""""|}, userId);",
            @"_logger.LogInformation(""""""Raw {UserId} value"""""", userId);");

        yield return Case(
            "Fix all across multiple templates in one file",
            @"_logger.LogInformation({|S6678:""{service} started for {userId}""|}, service, userId);
            _logger.LogWarning({|S6678:""{service} slow: {price,8:F2}""|}, service, price);
            _seriLogger.Error({|S6678:""{method} failed for {@propertyId}""|}, method, propertyId);",
            @"_logger.LogInformation(""{Service} started for {UserId}"", service, userId);
            _logger.LogWarning(""{Service} slow: {Price,8:F2}"", service, price);
            _seriLogger.Error(""{Method} failed for {@PropertyId}"", method, propertyId);");
    }

    private static TestCaseData Case(string name, string body, string fixedBody)
        => new TestCaseData(Wrap(body), Wrap(fixedBody)).SetName(name);

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public async Task DiagnosticOnWholeLiteral(string source, string fixedSource)
    {
        await new CodeFixTest<TemplateLiteralStubAnalyzer>(source, fixedSource).RunAsync(CancellationToken.None);
    }

    /// <summary>
    /// Sonar may report on the individual placeholder rather than on the whole literal, so make sure we locate
    /// the enclosing literal from a span that sits inside the string token - and that two diagnostics on the
    /// same literal produce non-overlapping fixes that Fix All can batch.
    /// </summary>
    [Test]
    public async Task DiagnosticOnPlaceholderOnly()
    {
        var source = Wrap(@"_logger.LogInformation(""{|S6678:{service}|}.{|S6678:{method}|} failed for {|S6678:{userId}|}"", service, method, userId);");
        var fixedSource = Wrap(@"_logger.LogInformation(""{Service}.{Method} failed for {UserId}"", service, method, userId);");

        await new CodeFixTest<PlaceholderStubAnalyzer>(source, fixedSource).RunAsync(CancellationToken.None);
    }

    /// <summary>
    /// When the diagnostic cannot be resolved to a string literal, the fixer must register no fix rather than
    /// throw. No <c>FixedCode</c> is supplied, so the source (and the diagnostic) must survive untouched.
    /// </summary>
    [Test]
    public async Task DiagnosticWithNoResolvableLiteralRegistersNoFix()
    {
        var source = Wrap(@"{|S6678:_logger.LogInformation(message, userId)|};");

        await new CodeFixTest<NonLiteralStubAnalyzer>(source, null).RunAsync(CancellationToken.None);
    }

    private class CodeFixTest<TAnalyzer> : CSharpCodeFixTest<TAnalyzer, S6678PascalCasePlaceholderCodeFixProvider, NUnitVerifier>
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        public CodeFixTest(string source, string fixedSource)
        {
            TestCode = source;
            if (fixedSource != null)
            {
                FixedCode = fixedSource;
            }
            else
            {
                // The source is expected to survive the fixer untouched, diagnostic and all.
                FixedState.MarkupHandling = MarkupMode.Allow;
            }

            ReferenceAssemblies = ReferenceAssemblies.Default
                .AddPackages(ImmutableArray.Create(
                    new PackageIdentity("Microsoft.Extensions.Logging.Abstractions", "6.0.0"),
                    new PackageIdentity("Serilog", "2.10.0")
                ));
        }
    }

    // ---------------------------------------------------------------------------------------------------
    // Test-only stubs standing in for SonarAnalyzer.CSharp's S6678. These analyzers never leave the test
    // assembly, so the rules that police shipped analyzer projects do not apply to them:
    //   RS1036 - EnforceExtendedAnalyzerRules is for projects that package analyzers; this one packages tests.
    //   RS2008 - S6678 is owned by SonarAnalyzer.CSharp, so it is not tracked in our release files.
    // ---------------------------------------------------------------------------------------------------
#pragma warning disable RS1036
#pragma warning disable RS2008

    private static readonly DiagnosticDescriptor S6678Rule = new DiagnosticDescriptor(
        "S6678",
        "Use PascalCase for named placeholders",
        "Use PascalCase for named placeholders",
        "Test",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// Matches a named placeholder whose name starts with a lower case letter, allowing for the "@"/"$"
    /// prefix and for alignment/format clauses. Escaped braces are masked out before matching.
    /// </summary>
    private static readonly Regex CamelCasePlaceholder =
        new Regex(@"\{[@$]?[a-z][A-Za-z0-9_]*(?:,[^}]*)?(?::[^}]*)?\}", RegexOptions.Compiled);

    private static IEnumerable<Match> FindCamelCasePlaceholders(string literalText)
    {
        // "{{" and "}}" are escaped literal braces in a message template, never placeholders. Replacing them
        // with same-length filler keeps every match index aligned with the original text.
        var masked = literalText.Replace("{{", "\u0000\u0000").Replace("}}", "\u0000\u0000");
        return CamelCasePlaceholder.Matches(masked).Cast<Match>();
    }

    private static bool IsLoggingCall(InvocationExpressionSyntax invocation)
    {
        var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
        if (memberAccess == null)
        {
            return false;
        }

        var name = memberAccess.Name.Identifier.ValueText;
        return name.StartsWith("Log")
               || name == "Information" || name == "Warning" || name == "Error"
               || name == "Debug" || name == "Verbose" || name == "Fatal";
    }

    private static IEnumerable<LiteralExpressionSyntax> StringLiteralArguments(InvocationExpressionSyntax invocation)
        => invocation.ArgumentList.Arguments
            .Select(a => a.Expression as LiteralExpressionSyntax)
            .Where(l => l != null && l.IsKind(SyntaxKind.StringLiteralExpression));

    public abstract class StubAnalyzerBase : DiagnosticAnalyzer
    {
        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(S6678Rule);

        public sealed override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.InvocationExpression);
        }

        protected abstract void Report(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation);

        private void Analyze(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            if (IsLoggingCall(invocation))
            {
                Report(context, invocation);
            }
        }
    }

    /// <summary>Reports on the whole message template literal, which is how Sonar reports S6678.</summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TemplateLiteralStubAnalyzer : StubAnalyzerBase
    {
        protected override void Report(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation)
        {
            foreach (var literal in StringLiteralArguments(invocation))
            {
                if (FindCamelCasePlaceholders(literal.Token.Text).Any())
                {
                    context.ReportDiagnostic(Diagnostic.Create(S6678Rule, literal.GetLocation()));
                }
            }
        }
    }

    /// <summary>Reports one diagnostic per offending placeholder, located inside the literal token.</summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PlaceholderStubAnalyzer : StubAnalyzerBase
    {
        protected override void Report(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation)
        {
            foreach (var literal in StringLiteralArguments(invocation))
            {
                foreach (var match in FindCamelCasePlaceholders(literal.Token.Text))
                {
                    var span = new TextSpan(literal.Token.SpanStart + match.Index, match.Length);
                    context.ReportDiagnostic(Diagnostic.Create(S6678Rule, Location.Create(literal.SyntaxTree, span)));
                }
            }
        }
    }

    /// <summary>Reports on a logging call that has no string literal at all, to exercise the bail-out path.</summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NonLiteralStubAnalyzer : StubAnalyzerBase
    {
        protected override void Report(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation)
        {
            if (!StringLiteralArguments(invocation).Any())
            {
                context.ReportDiagnostic(Diagnostic.Create(S6678Rule, invocation.GetLocation()));
            }
        }
    }

#pragma warning restore RS2008
#pragma warning restore RS1036
}
