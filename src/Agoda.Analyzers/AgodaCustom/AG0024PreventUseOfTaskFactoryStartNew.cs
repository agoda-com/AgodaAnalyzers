using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Agoda.Analyzers.Helpers;
using System.Threading.Tasks;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0024PreventUseOfTaskFactoryStartNew : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0024";

        private static readonly LocalizableString info = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0024Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description = DescriptionContentLoader.GetAnalyzerDescription(
            nameof(AG0024PreventUseOfTaskFactoryStartNew));

        private static DiagnosticDescriptor Descriptor => new DiagnosticDescriptor(
            DIAGNOSTIC_ID,
            info,
            info,
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Warning,
            AnalyzerConstants.EnabledByDefault,
            Description,
            null,
            WellKnownDiagnosticTags.EditAndContinue);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            string factoryNamespace = "System.Threading.Tasks.TaskFactory";
            string callExpression = "StartNew";
            string allowedParameter = "TaskCreationOptions.LongRunning";

            var invocationExpressionSyntax = (InvocationExpressionSyntax)context.Node;
            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol as IMethodSymbol;

            //In order to show warning, If has to be a method
            if (methodSymbol == null) return;

            //It has to be of TaskFactory type
            if(methodSymbol.ContainingType.ToDisplayString() != factoryNamespace) return;

            //It has to contain call to StartNew
            if (methodSymbol.Name != callExpression) return;

            //Should not be long running task
            var isLongRunning = invocationExpressionSyntax.ArgumentList.Arguments.Any(a =>
                a.ToFullString().Contains(allowedParameter)
            );
            if (isLongRunning) return;
            
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation()));

        }
    }
}