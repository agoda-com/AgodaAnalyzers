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
    public class AG0043NoBuildServiceProvider : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0043";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0043Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0043Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description
            = DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0043NoBuildServiceProvider));

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DIAGNOSTIC_ID,
            Title,
            MessageFormat,
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Error,
            AnalyzerConstants.EnabledByDefault,
            Description,
            "https://github.com/agoda-com/AgodaAnalyzers/issues/190",
            WellKnownDiagnosticTags.EditAndContinue);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var invocationExpr = (InvocationExpressionSyntax)context.Node;

            // Check if it's a member access (e.g., something.BuildServiceProvider())
            if (!(invocationExpr.Expression is MemberAccessExpressionSyntax memberAccessExpr))
                return;

            // Check if the method name is BuildServiceProvider
            if (memberAccessExpr.Name.Identifier.ValueText != "BuildServiceProvider")
                return;

            // Get the method symbol
            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocationExpr).Symbol as IMethodSymbol;
            if (methodSymbol == null)
                return;

            // Verify it's from the correct namespace
            if (!methodSymbol.ContainingNamespace.ToDisplayString()
                    .StartsWith("Microsoft.Extensions.DependencyInjection"))
                return;

            // Get the containing type that defines the BuildServiceProvider method
            var containingType = methodSymbol.ContainingType;
            if (containingType == null)
                return;

            // Check if the containing type is an extension method class for IServiceCollection
            var isServiceCollectionExtension = containingType
                .GetTypeMembers()
                .SelectMany(t => t.GetMembers())
                .OfType<IMethodSymbol>()
                .Any(m => m.Parameters.Length > 0 &&
                          m.Parameters[0].Type.ToDisplayString() ==
                          "Microsoft.Extensions.DependencyInjection.IServiceCollection");

            // For extension methods, check the type of the expression being extended
            var expressionType = context.SemanticModel.GetTypeInfo(memberAccessExpr.Expression).Type;
            var isExpressionServiceCollection = expressionType != null &&
                                                (expressionType.AllInterfaces.Any(i =>
                                                     i.ToDisplayString() ==
                                                     "Microsoft.Extensions.DependencyInjection.IServiceCollection") ||
                                                 expressionType.ToDisplayString() ==
                                                 "Microsoft.Extensions.DependencyInjection.IServiceCollection");

            if (isServiceCollectionExtension || isExpressionServiceCollection)
            {
                var diagnostic = Diagnostic.Create(Descriptor, memberAccessExpr.Name.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}