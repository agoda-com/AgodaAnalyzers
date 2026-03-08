using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        private const int SafeYearThreshold = 2020;

        private static readonly Regex DateStringPattern = new Regex(
            @"^\d{4}[-/]\d{1,2}[-/]\d{1,2}($|[T\s])",
            RegexOptions.Compiled);

        private static readonly HashSet<string> TestClassAttributes = new HashSet<string>
        {
            "NUnit.Framework.TestFixtureAttribute",
            "Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute",
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

            var yearArg = arguments.Value[0].Expression as LiteralExpressionSyntax;
            if (yearArg == null || !yearArg.IsKind(SyntaxKind.NumericLiteralExpression))
                return;

            var year = (int)yearArg.Token.Value;
            if (year < SafeYearThreshold)
                return;

            context.ReportDiagnostic(Diagnostic.Create(Rule, creation.GetLocation(), Properties));
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

            if (TryExtractYear(dateString, out var year) && year < SafeYearThreshold)
                return;

            context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), Properties));
        }

        private static bool IsInTestContext(SyntaxNodeAnalysisContext context)
        {
            var containingSymbol = context.ContainingSymbol;
            var ns = containingSymbol?.ContainingNamespace?.ToDisplayString();
            if (ns != null && HasTestNamespaceSegment(ns))
                return true;

            var containingType = containingSymbol?.ContainingType ?? containingSymbol as INamedTypeSymbol;
            while (containingType != null)
            {
                if (HasTestAttribute(containingType))
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
                    segment.EndsWith("Tests", StringComparison.OrdinalIgnoreCase) ||
                    segment.StartsWith("Test", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static bool HasTestAttribute(INamedTypeSymbol type)
        {
            return type.GetAttributes().Any(attr =>
                TestClassAttributes.Contains(attr.AttributeClass?.ToString()));
        }

        private static bool AllArgumentsAreLiterals(IEnumerable<ArgumentSyntax> arguments)
        {
            return arguments.All(arg => arg.Expression is LiteralExpressionSyntax literal
                                        && literal.IsKind(SyntaxKind.NumericLiteralExpression));
        }

        private static bool TryExtractYear(string dateString, out int year)
        {
            year = 0;
            var dashIndex = dateString.IndexOfAny(new[] { '-', '/' });
            if (dashIndex <= 0)
                return false;

            return int.TryParse(dateString.Substring(0, dashIndex), out year);
        }
    }
}
