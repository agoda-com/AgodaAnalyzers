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
    public class AG0008PreventReturningNullForReturnValueOfIEnumerable : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AG0008";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0008Title), CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0008Title), CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description =
            DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0008PreventReturningNullForReturnValueOfIEnumerable));

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, AnalyzerCategory.CustomQualityRules,
                DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, null,
                WellKnownDiagnosticTags.EditAndContinue);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, new[] { SyntaxKind.ReturnKeyword, SyntaxKind.ReturnStatement, SyntaxKind.NullLiteralExpression });
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.IsKind(SyntaxKind.ReturnStatement))
            {
                ReturnStatementSyntax statement = (ReturnStatementSyntax)context.Node;
                if (statement.Expression.IsKind(SyntaxKind.NullLiteralExpression)) // will fail to catch the tertiary operator with null value
                {
                    if (context.ContainingSymbol.Kind == SymbolKind.Method)
                    {
                        IMethodSymbol method = (IMethodSymbol)context.ContainingSymbol;
                        var methodReturnType = method.ReturnType as INamedTypeSymbol;
                        if (methodReturnType?.ConstructedFrom.Interfaces.Any(x => x.ToDisplayString() == "System.Collections.IEnumerable") == true
                            && methodReturnType.ConstructedFrom.ToDisplayString() != "string")
                        {   
                            context.ReportDiagnostic(Diagnostic.Create(Descriptor, statement.GetLocation()));
                        }
                    }
                }
            }
        }
    }
}