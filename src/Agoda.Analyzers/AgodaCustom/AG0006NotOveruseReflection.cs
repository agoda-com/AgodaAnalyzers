using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0006NotOveruseReflection : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AG0006";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(CustomRulesResources.AG0006Title), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(CustomRulesResources.AG0006Title), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));
        private static readonly LocalizableString Description = DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0006NotOveruseReflection));

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, AnalyzerCategory.CustomQualityRules,
                DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, null, WellKnownDiagnosticTags.EditAndContinue);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Descriptor); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is InvocationExpressionSyntax)) return;

            var invocation = (InvocationExpressionSyntax)context.Node;

            if (invocation?.Expression == null) return;

            if (!(context.SemanticModel.GetSymbolInfo(invocation.Expression).Symbol is IMethodSymbol symbol)) return;

            var typeInfo = context.SemanticModel.GetTypeInfo(context.Node);
            var typeName = typeInfo.Type.ToDisplayString();

            if (symbol.Name == "CreateInstance" && typeName == "object")
            {
                var diagnostic = Diagnostic.Create(Descriptor, context.Node.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
            else if (symbol.Name == "Load" && typeName == "System.Reflection.Assembly")
            {
                var diagnostic = Diagnostic.Create(Descriptor, context.Node.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
