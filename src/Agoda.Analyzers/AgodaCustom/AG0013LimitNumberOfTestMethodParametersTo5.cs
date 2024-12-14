using Microsoft.CodeAnalysis;
using Agoda.Analyzers.Helpers;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0013LimitNumberOfTestMethodParametersTo5 : DiagnosticAnalyzer
    {
        private const int MAXIMUM_TEST_PARAMETERS = 5;
        public const string DIAGNOSTIC_ID = "AG0013";
        private readonly DiagnosticDescriptor _diagnosticDescriptor;

        public AG0013LimitNumberOfTestMethodParametersTo5()
        {
            var info = new LocalizableResourceString(
                nameof(CustomRulesResources.AG0013Title), 
                CustomRulesResources.ResourceManager, 
                typeof(CustomRulesResources));

            _diagnosticDescriptor = new DiagnosticDescriptor(
                DIAGNOSTIC_ID, 
                info,
                info,
                AnalyzerCategory.CustomQualityRules,
                DiagnosticSeverity.Warning, 
                AnalyzerConstants.EnabledByDefault, 
                DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0013LimitNumberOfTestMethodParametersTo5)),
                $"https://github.com/agoda-com/AgodaAnalyzers/blob/master/doc/{DIAGNOSTIC_ID}.md", 
                WellKnownDiagnosticTags.EditAndContinue
            );
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_diagnosticDescriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax) context.Node;

            if (!TestMethodHelpers.IsTestCase(methodDeclaration, context)) { return; }
                
            if(!IsTestPrametersMoreThanLimit(methodDeclaration)) { return; }

            context.ReportDiagnostic(Diagnostic.Create(_diagnosticDescriptor, methodDeclaration.GetLocation(),_props.ToImmutableDictionary()));
        }

        private bool IsTestPrametersMoreThanLimit(MethodDeclarationSyntax method) => (method.ParameterList?.Parameters.Count ?? 0) > MAXIMUM_TEST_PARAMETERS;

        private static Dictionary<string, string> _props = new Dictionary<string, string>()
        {
            { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "10" }
        };
    }
}
