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
        public const string DIAGNOSTIC_ID = "AG0020";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0020Title), 
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0020Title), 
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description 
            = DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0020AvoidReturningNullEnumerables));

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DIAGNOSTIC_ID, 
            Title,
            MessageFormat, 
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Warning, 
            AnalyzerConstants.EnabledByDefault, 
            Description, 
            "https://agoda-com.github.io/standards-c-sharp/collections/null-empty-enumerables.html",
            WellKnownDiagnosticTags.EditAndContinue);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
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

            if (expression.IsKind(SyntaxKind.ConditionalExpression))
            {
                var statement = expression as ConditionalExpressionSyntax;
                return GetNullLiteralLocation(statement.WhenTrue) ?? GetNullLiteralLocation(statement.WhenFalse);
            }
            return null;
        }

        private Location IsReturningNullLiteral(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.IsKind(SyntaxKind.ReturnStatement))
            {
                if (!(context.Node is ReturnStatementSyntax statement)) return null;
                return GetNullLiteralLocation(statement.Expression);
            }

            if (context.Node.IsKind(SyntaxKind.ArrowExpressionClause))
            {
                var statement = context.Node as ArrowExpressionClauseSyntax;
                return GetNullLiteralLocation(statement.Expression);
            }
            return null;
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var location = IsReturningNullLiteral(context);
            if (location != null && context.ContainingSymbol.Kind == SymbolKind.Method)
            {
                var method = (IMethodSymbol)context.ContainingSymbol;
                var isArray = method.ReturnType is IArrayTypeSymbol;
                if (isArray ||
                    (method.ReturnType is INamedTypeSymbol methodReturnType
                     && methodReturnType.ConstructedFrom.Interfaces.Any(x => x.ToDisplayString() == "System.Collections.IEnumerable")
                     && methodReturnType.ConstructedFrom.ToDisplayString() != "string"))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
                }

            }
        }
    }
}