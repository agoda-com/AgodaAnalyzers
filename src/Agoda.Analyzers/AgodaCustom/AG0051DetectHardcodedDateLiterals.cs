using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0051DetectHardcodedDateLiterals : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AG0051";
        public const string ConfidencePropertyName = "confidence";
        public const string HighConfidence = "high";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0051Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0051MessageFormat),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0051Description),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Warning,
            AnalyzerConstants.EnabledByDefault,
            Description,
            $"https://github.com/agoda-com/AgodaAnalyzers/blob/master/doc/{DiagnosticId}.md",
            WellKnownDiagnosticTags.EditAndContinue);

        private static readonly ImmutableDictionary<string, string> Properties =
            new Dictionary<string, string>
            {
                { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "10" }
            }.ToImmutableDictionary();

        private const int SentinelYearThreshold = 2999;

        private static readonly DateTime AnalysisDate = DateTime.UtcNow.Date;

        private static readonly Regex DateStringPattern = new Regex(
            @"^\d{4}[-/]\d{1,2}[-/]\d{1,2}($|[T\s])",
            RegexOptions.Compiled);

        private static readonly HashSet<string> TestClassAttributes = new HashSet<string>
        {
            "NUnit.Framework.TestFixtureAttribute",
            "Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute",
        };

        private static readonly HashSet<string> AssertionMethods = new HashSet<string>
        {
            "ShouldBe",
            "ShouldBeEquivalentTo",
            "AreEqual",
            "Equal",
            "Be",
            "BeEquivalentTo",
        };

        private static readonly HashSet<string> RiskyPropertyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "StartDate",
            "EndDate",
            "From",
            "To",
            "Expiry",
            "Expiration",
            "ExpiresAt",
            "CheckIn",
            "CheckOut",
            "CheckInDate",
            "CheckOutDate",
            "ValidFrom",
            "ValidTo",
            "PayableDate",
            "FirstLiveDate",
            "EffectiveDate",
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
        {
            if (!IsInTestContext(context))
                return;

            var creation = (ObjectCreationExpressionSyntax)context.Node;
            var typeInfo = context.SemanticModel.GetTypeInfo(creation);
            var typeSymbol = typeInfo.Type;

            if (typeSymbol == null)
                return;

            var fullTypeName = typeSymbol.ToDisplayString();
            if (fullTypeName != "System.DateTime" && fullTypeName != "System.DateTimeOffset")
                return;

            var arguments = creation.ArgumentList?.Arguments;
            if (arguments == null || arguments.Value.Count < 3)
                return;

            if (!AllArgumentsAreLiterals(arguments.Value.Take(3)))
                return;

            if (!TryGetDateParts(arguments.Value, out var year, out var month, out var day))
                return;

            if (IsSentinelDate(year) || IsPastDate(year, month, day) || IsLowRiskUsage(creation))
                return;

            context.ReportDiagnostic(CreateDiagnostic(creation));
        }

        private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            if (!IsInTestContext(context))
                return;

            var invocation = (InvocationExpressionSyntax)context.Node;

            if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess))
                return;

            var methodName = memberAccess.Name.Identifier.Text;
            if (methodName != "Parse" && methodName != "TryParse")
                return;

            var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
            var methodSymbol = symbolInfo.Symbol as IMethodSymbol;
            if (methodSymbol == null)
                return;

            var containingType = methodSymbol.ContainingType?.ToDisplayString();
            if (containingType != "System.DateTime" && containingType != "System.DateTimeOffset")
                return;

            var arguments = invocation.ArgumentList?.Arguments;
            if (arguments == null || arguments.Value.Count < 1)
                return;

            var firstArg = arguments.Value[0].Expression as LiteralExpressionSyntax;
            if (firstArg == null || !firstArg.IsKind(SyntaxKind.StringLiteralExpression))
                return;

            var dateString = firstArg.Token.ValueText;
            if (!DateStringPattern.IsMatch(dateString))
                return;

            if (!DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                return;

            if (IsSentinelDate(parsedDate.Year) ||
                IsPastDate(parsedDate.Year, parsedDate.Month, parsedDate.Day) ||
                IsLowRiskUsage(invocation))
                return;

            context.ReportDiagnostic(CreateDiagnostic(invocation));
        }

        private static Diagnostic CreateDiagnostic(SyntaxNode node)
        {
            return Diagnostic.Create(
                Rule,
                node.GetLocation(),
                Properties.Add(ConfidencePropertyName, HighConfidence));
        }

        private static bool IsInTestContext(SyntaxNodeAnalysisContext context)
        {
            var containingSymbol = context.ContainingSymbol;
            var ns = containingSymbol?.ContainingNamespace?.ToDisplayString();
            if (ns != null && HasTestNamespaceSegment(ns))
                return true;

            if (containingSymbol is IMethodSymbol method && HasTestMethodAttribute(method))
                return true;

            var containingType = containingSymbol?.ContainingType ?? containingSymbol as INamedTypeSymbol;
            while (containingType != null)
            {
                if (HasTestClassAttribute(containingType))
                    return true;
                containingType = containingType.ContainingType;
            }

            return false;
        }

        private static bool HasTestNamespaceSegment(string ns)
        {
            foreach (var segment in ns.Split('.'))
            {
                if (segment.EndsWith("Test", StringComparison.OrdinalIgnoreCase) ||
                    segment.EndsWith("Tests", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static readonly HashSet<string> TestMethodAttributes = new HashSet<string>
        {
            "NUnit.Framework.TestAttribute",
            "Xunit.FactAttribute",
            "Xunit.TheoryAttribute",
            "Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute",
        };

        private static bool HasTestClassAttribute(INamedTypeSymbol type)
        {
            return type.GetAttributes().Any(attr =>
                TestClassAttributes.Contains(attr.AttributeClass?.ToString()));
        }

        private static bool HasTestMethodAttribute(IMethodSymbol method)
        {
            return method.GetAttributes().Any(attr =>
                TestMethodAttributes.Contains(attr.AttributeClass?.ToString()));
        }

        private static bool AllArgumentsAreLiterals(IEnumerable<ArgumentSyntax> arguments)
        {
            return arguments.All(arg => arg.Expression is LiteralExpressionSyntax literal
                                        && literal.IsKind(SyntaxKind.NumericLiteralExpression));
        }

        private static bool TryGetDateParts(SeparatedSyntaxList<ArgumentSyntax> arguments, out int year, out int month, out int day)
        {
            year = 0;
            month = 0;
            day = 0;

            return TryGetIntLiteral(arguments[0], out year) &&
                   TryGetIntLiteral(arguments[1], out month) &&
                   TryGetIntLiteral(arguments[2], out day);
        }

        private static bool TryGetIntLiteral(ArgumentSyntax argument, out int value)
        {
            value = 0;
            var literal = argument.Expression as LiteralExpressionSyntax;
            if (literal == null || !literal.IsKind(SyntaxKind.NumericLiteralExpression))
                return false;

            if (literal.Token.Value is int intValue)
            {
                value = intValue;
                return true;
            }

            return false;
        }

        private static bool IsSentinelDate(int year)
        {
            return year >= SentinelYearThreshold;
        }

        private static bool IsPastDate(int year, int month, int day)
        {
            if (year < 1 || year > 9999 || month < 1 || month > 12 || day < 1 || day > 31)
                return false;

            return new DateTime(year, month, 1).AddMonths(1) < AnalysisDate;
        }

        private static bool IsLowRiskUsage(SyntaxNode node)
        {
            if (IsLowRiskObjectInitializerAssignment(node))
                return true;

            var argument = node.Parent as ArgumentSyntax;
            var invocation = argument?.Parent?.Parent as InvocationExpressionSyntax;
            return invocation != null &&
                   TryGetInvokedName(invocation, out var invokedName) &&
                   AssertionMethods.Contains(invokedName);
        }

        private static bool IsLowRiskObjectInitializerAssignment(SyntaxNode node)
        {
            var assignment = node.Parent as AssignmentExpressionSyntax;
            if (assignment == null || !(assignment.Parent is InitializerExpressionSyntax))
                return false;

            var identifierName = assignment.Left as IdentifierNameSyntax;
            return identifierName != null && !RiskyPropertyNames.Contains(identifierName.Identifier.Text);
        }

        private static bool TryGetInvokedName(InvocationExpressionSyntax invocation, out string name)
        {
            name = null;

            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccess != null)
            {
                name = memberAccess.Name.Identifier.Text;
                return !string.IsNullOrEmpty(name);
            }

            var identifierName = invocation.Expression as IdentifierNameSyntax;
            if (identifierName != null)
            {
                name = identifierName.Identifier.Text;
                return !string.IsNullOrEmpty(name);
            }

            return false;
        }
    }
}
