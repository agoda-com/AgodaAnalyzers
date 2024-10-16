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
            "https://playwright.dev/dotnet/docs/api/class-elementhandle",
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

            var memberAccess = invocationExpression.Expression as MemberAccessExpressionSyntax;
            if (memberAccess == null)
                return;

            var methodName = memberAccess.Name.Identifier.Text;

            // Check if the method is QuerySelectorAsync
            if (methodName != "QuerySelectorAsync")
                return;

            // Get the semantic model to resolve types
            var semanticModel = context.SemanticModel;

            // Get the type of the object calling the method (e.g., 'page' in 'page.QuerySelectorAsync')
            var symbolInfo = semanticModel.GetSymbolInfo(memberAccess.Expression, context.CancellationToken);
            var symbol = symbolInfo.Symbol;

            if (symbol == null)
                return;

            // Check if it's a local variable, parameter, field, or property
            INamedTypeSymbol typeSymbol = null;

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
            }
            
            if (typeSymbol == null)
                return;

            // Check if the type is Playwright's Page or IPage
            if (typeSymbol.ToString() != "Microsoft.Playwright.IPage" &&
                typeSymbol.ToString() != "Microsoft.Playwright.Page")
                return;

            // Report a diagnostic at the location of this method call
            var diagnostic = Diagnostic.Create(Descriptor, invocationExpression.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}