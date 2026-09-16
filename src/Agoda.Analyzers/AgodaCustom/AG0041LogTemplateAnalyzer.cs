using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0041LogTemplateAnalyzer : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0041";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(CustomRulesResources.AG0041Title), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(CustomRulesResources.AG0041Title), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));
        private const string Category = "Best Practices";

        internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DIAGNOSTIC_ID,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: $"https://github.com/agoda-com/AgodaAnalyzers/blob/master/doc/{DIAGNOSTIC_ID}.md");

        /// <summary>
        /// Types whose members are treated as logging entry points. The check is deliberately
        /// narrow: it is the *declaring type of the invoked method* that must match, so unrelated
        /// types that happen to expose an <c>Information(...)</c> member are never reported.
        /// </summary>
        private static readonly ImmutableHashSet<string> LoggingTypeNames = ImmutableHashSet.Create(
            "Serilog.ILogger",
            "Serilog.Log",
            "Serilog.LoggerExtensions",
            "Microsoft.Extensions.Logging.ILogger",
            "Microsoft.Extensions.Logging.LoggerExtensions");

        /// <summary>
        /// Logger interfaces; any type implementing one of these is also treated as a logger, which
        /// covers concrete implementations such as <c>Serilog.Core.Logger</c>.
        /// </summary>
        private static readonly ImmutableHashSet<string> LoggerInterfaceNames = ImmutableHashSet.Create(
            "Serilog.ILogger",
            "Microsoft.Extensions.Logging.ILogger");

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

            if (!IsLoggingMethod(invocation, context.SemanticModel))
                return;

            var argument = FindTemplateArgument(invocation, context.SemanticModel);
            if (argument == null)
                return;

            if (argument.Expression.IsKind(SyntaxKind.InterpolatedStringExpression) ||
                IsStringConcatenation(argument.Expression))
            {
                var diagnostic = Diagnostic.Create(Rule, argument.GetLocation(), properties: _props.ToImmutableDictionary());
                context.ReportDiagnostic(diagnostic);
            }
        }

        /// <summary>
        /// Finds the message-template argument of a logging call. The template is the first argument
        /// that is a string literal, an interpolated string or a string concatenation; leading
        /// <c>Exception</c> and <c>EventId</c> arguments are skipped so that the exception-first and
        /// EventId-first overloads are covered. Anything else stops the search, so we never report on
        /// an argument that is not in message-template position.
        /// </summary>
        internal static ArgumentSyntax FindTemplateArgument(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            if (invocation.ArgumentList == null)
                return null;

            foreach (var argument in invocation.ArgumentList.Arguments)
            {
                if (IsTemplateShaped(argument.Expression))
                    return argument;

                if (IsExceptionOrEventId(argument.Expression, semanticModel))
                    continue;

                return null;
            }

            return null;
        }

        private static bool IsTemplateShaped(ExpressionSyntax expression)
        {
            return expression.IsKind(SyntaxKind.InterpolatedStringExpression)
                   || expression.IsKind(SyntaxKind.StringLiteralExpression)
                   || IsStringConcatenation(expression);
        }

        /// <summary>
        /// True when the expression is itself a concatenation chain containing at least one string
        /// literal. Unlike a descendant search this cannot be fooled by a concatenation buried
        /// inside an unrelated argument, e.g. <c>logger.Error(new Exception("a" + b), template)</c>.
        /// </summary>
        private static bool IsStringConcatenation(ExpressionSyntax expression)
        {
            var binary = expression as BinaryExpressionSyntax;
            return binary != null
                   && binary.IsKind(SyntaxKind.AddExpression)
                   && ContainsStringConcatenation(binary);
        }

        private static bool IsExceptionOrEventId(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            var typeInfo = semanticModel.GetTypeInfo(expression);
            return IsExceptionOrEventId(typeInfo.Type) || IsExceptionOrEventId(typeInfo.ConvertedType);
        }

        private static bool IsExceptionOrEventId(ITypeSymbol type)
        {
            if (type == null)
                return false;

            if (type.Name == "EventId" && type.ContainingNamespace?.ToString() == "Microsoft.Extensions.Logging")
                return true;

            for (var current = type; current != null; current = current.BaseType)
            {
                if (current.Name == "Exception" && current.ContainingNamespace?.ToString() == "System")
                    return true;
            }

            return false;
        }

        private static bool IsLoggingMethod(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(invocation);
            var method = symbolInfo.Symbol as IMethodSymbol;

            if (method == null)
            {
                // Overload resolution can fail while the user is still typing; fall back to the
                // candidates so the rule stays useful in a partially broken document.
                method = symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();
            }

            if (method == null)
                return false;

            // For an extension method invoked in reduced form the containing type is still the
            // static class that declares it (e.g. Microsoft.Extensions.Logging.LoggerExtensions).
            return IsLoggerType(method.ContainingType);
        }

        private static bool IsLoggerType(INamedTypeSymbol type)
        {
            if (type == null)
                return false;

            if (LoggingTypeNames.Contains(GetFullName(type)))
                return true;

            return type.AllInterfaces.Any(i => LoggerInterfaceNames.Contains(GetFullName(i)));
        }

        private static string GetFullName(INamedTypeSymbol type)
        {
            var containingNamespace = type.ContainingNamespace;
            if (containingNamespace == null || containingNamespace.IsGlobalNamespace)
                return type.Name;

            return containingNamespace.ToDisplayString() + "." + type.Name;
        }

        private static bool ContainsStringConcatenation(ExpressionSyntax expression)
        {
            return expression.DescendantNodesAndSelf().OfType<BinaryExpressionSyntax>()
                .Any(bes => bes.IsKind(SyntaxKind.AddExpression) &&
                            (bes.Left.IsKind(SyntaxKind.StringLiteralExpression) ||
                             bes.Right.IsKind(SyntaxKind.StringLiteralExpression)));
        }

        private static Dictionary<string, string> _props = new Dictionary<string, string>()
        {
            { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "10" }
        };
    }
}
