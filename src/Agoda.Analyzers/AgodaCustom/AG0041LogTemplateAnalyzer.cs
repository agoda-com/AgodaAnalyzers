﻿using System.Collections.Generic;
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

            var argument = invocation.ArgumentList.Arguments.FirstOrDefault();
            if (argument == null)
                return;

            if (argument.Expression.IsKind(SyntaxKind.InterpolatedStringExpression) ||
                ContainsStringConcatenation(argument.Expression))
            {
                var diagnostic = Diagnostic.Create(Rule, argument.GetLocation(), properties: _props.ToImmutableDictionary());
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsLoggingMethod(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(memberAccess.Expression);
                var symbol = symbolInfo.Symbol;

                if (symbol is IFieldSymbol fieldSymbol)
                {
                    var type = fieldSymbol.Type;
                    return IsILogger(type) || IsSerilog(type);
                }
            }
            return false;
        }

        private static bool IsILogger(ITypeSymbol type)
        {
            return type.Name == "ILogger" && type.ContainingNamespace?.ToString() == "Microsoft.Extensions.Logging";
        }

        private static bool IsSerilog(ITypeSymbol type)
        {
            return type.Name == "ILogger" && type.ContainingNamespace?.ToString() == "Serilog";
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
