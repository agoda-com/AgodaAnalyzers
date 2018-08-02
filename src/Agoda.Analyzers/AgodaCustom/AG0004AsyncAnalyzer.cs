using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Agoda.Analyzers.Helpers;
using System.Collections.Generic;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0004AsyncAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AG0004";

        private readonly List<string> blockingMethodCallsOnTask = new List<string> { "Wait", "WaitAny", "WaitAll" };

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(CustomRulesResources.AG0004Title), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(CustomRulesResources.AG0004MessageFormat), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));
        private static readonly LocalizableString Description = DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0004AsyncAnalyzer));

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, AnalyzerCategory.CustomQualityRules,
                DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, null, WellKnownDiagnosticTags.EditAndContinue);


        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.SimpleMemberAccessExpression);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Descriptor); } }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var memberAccessNode = (MemberAccessExpressionSyntax)context.Node;

            var method = context.SemanticModel.GetTypeInfo(memberAccessNode);

            var invokeMethod = context.SemanticModel.GetSymbolInfo(context.Node).Symbol as IMethodSymbol;

            if (invokeMethod.ContainingType.Name != "Task") return;

            var location = memberAccessNode.Name.GetLocation();

            if (invokeMethod != null && !invokeMethod.IsExtensionMethod)
            {

                // Checks if the Wait method is called within an async method then creates the diagnostic.
                if (blockingMethodCallsOnTask.Contains(invokeMethod.OriginalDefinition.Name))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
                    return;
                }
            }

            var property = context.SemanticModel.GetSymbolInfo(context.Node).Symbol as IPropertySymbol;

            // Checks if the Result property is called within an async method then creates the diagnostic.
            if (property != null && property.OriginalDefinition.Name.Equals("Result"))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
                return;
            }
        }
    }
}
