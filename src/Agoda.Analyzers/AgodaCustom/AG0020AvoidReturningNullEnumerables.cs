using System.Collections.Immutable;
using System.Linq;
using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0020AvoidReturningNullEnumerables : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AG0020";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0020Title), CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0020Title), CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description =
            DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0020AvoidReturningNullEnumerables));

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, AnalyzerCategory.CustomQualityRules,
                DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, null,
                WellKnownDiagnosticTags.EditAndContinue);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeNode, new[] {
                SyntaxKind.ReturnStatement,
                SyntaxKind.ArrowExpressionClause,
                SyntaxKind.ConditionalExpression
            });
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        private Location GetNullLiteralLocation(ExpressionSyntax expression)
        {
            if (expression.IsKind(SyntaxKind.NullLiteralExpression))
                return expression.GetLocation();
            return null;
        }

        private Location IsReturningNullLiteral(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.IsKind(SyntaxKind.ReturnStatement))
            {
                var statement = context.Node as ReturnStatementSyntax;
                return GetNullLiteralLocation(statement.Expression);
            }
            else if (context.Node.IsKind(SyntaxKind.ArrowExpressionClause))
            {
                var statement = context.Node as ArrowExpressionClauseSyntax;
                return GetNullLiteralLocation(statement.Expression);
            }
            else if (context.Node.IsKind(SyntaxKind.ConditionalExpression))
            {
                var statement = context.Node as ConditionalExpressionSyntax;
                return GetNullLiteralLocation(statement.WhenTrue) ?? GetNullLiteralLocation(statement.WhenFalse);
            }
            return null;
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            Location location = IsReturningNullLiteral(context);
            if (location != null && context.ContainingSymbol.Kind == SymbolKind.Method)
            {
                IMethodSymbol method = (IMethodSymbol)context.ContainingSymbol;
                var methodReturnType = method.ReturnType as INamedTypeSymbol;
                if ((methodReturnType?.ConstructedFrom.Interfaces.Any(x => x.ToDisplayString() == "System.Collections.IEnumerable")).Value
                    && methodReturnType.ConstructedFrom.ToDisplayString() != "string")
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
                }

            }
        }
    }
}