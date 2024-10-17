using System.Collections.Immutable;
using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0042QuerySelectorShouldNotBeUsed : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0042";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0042Title), CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0042Title), CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description
            = DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0042QuerySelectorShouldNotBeUsed));

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DIAGNOSTIC_ID,
            Title,
            MessageFormat,
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Warning,
            AnalyzerConstants.EnabledByDefault,
            Description,
            "https://github.com/agoda-com/AgodaAnalyzers/blob/master/doc/AG0042.md",
            WellKnownDiagnosticTags.EditAndContinue);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.InvocationExpression);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var invocationExpression = (InvocationExpressionSyntax)context.Node;

            if (!(invocationExpression.Expression is MemberAccessExpressionSyntax memberAccess))
                return;

            var methodName = memberAccess.Name.Identifier.Text;

            if (methodName != "QuerySelectorAsync")
                return;

            var semanticModel = context.SemanticModel;

            var symbolInfo = semanticModel.GetSymbolInfo(memberAccess.Expression, context.CancellationToken);
            var symbol = symbolInfo.Symbol;

            if (symbol == null)
                return;

            INamedTypeSymbol typeSymbol;
            switch (symbol)
            {
                case ILocalSymbol localSymbol:
                    typeSymbol = localSymbol.Type as INamedTypeSymbol;
                    break;
                case IParameterSymbol parameterSymbol:
                    typeSymbol = parameterSymbol.Type as INamedTypeSymbol;
                    break;
                case IFieldSymbol fieldSymbol:
                    typeSymbol = fieldSymbol.Type as INamedTypeSymbol;
                    break;
                case IPropertySymbol propertySymbol:
                    typeSymbol = propertySymbol.Type as INamedTypeSymbol;
                    break;
                default:
                    typeSymbol = null;
                    break;
            }

            if (typeSymbol == null)
                return;

            if (typeSymbol.ToString() != "Microsoft.Playwright.IPage" &&
                typeSymbol.ToString() != "Microsoft.Playwright.Page")
                return;

            var diagnostic = Diagnostic.Create(Descriptor, invocationExpression.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}