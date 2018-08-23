using Microsoft.CodeAnalysis;
using Agoda.Analyzers.Helpers;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0013LimitNumberOfTestMethodParametersTo5 : DiagnosticAnalyzer
    {
        private const string DIAGNOSTIC_ID = "AG0013";
        private readonly DiagnosticDescriptor _diagnosticDescriptor;

        public AG0013LimitNumberOfTestMethodParametersTo5()
        {
            var info = new LocalizableResourceString(nameof(CustomRulesResources.AG0013Title), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));

            _diagnosticDescriptor = new DiagnosticDescriptor(
                DIAGNOSTIC_ID, 
                info,
                info,
                AnalyzerCategory.CustomQualityRules,
                DiagnosticSeverity.Warning, 
                AnalyzerConstants.EnabledByDefault, 
                DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0013LimitNumberOfTestMethodParametersTo5)), 
                "https://github.agodadev.io/pages/standards-c-sharp/code-standards/unit-testing/use-test-cases-appropriately.html", 
                WellKnownDiagnosticTags.EditAndContinue
            );
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_diagnosticDescriptor);

        public override void Initialize(AnalysisContext context) => context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            // filter only test method
            var methodDeclaration = (MethodDeclarationSyntax) context.Node;
            if (!MethodHelper.IsTestCase(methodDeclaration, context)) { return; }
                
            // validate test method has no parameter more than 5 (todo-moch : check this)
            if(methodDeclaration.ParameterList.ChildNodes().ToImmutableList().Count < 6) { return; }
            
            // report error to visual studio
            context.ReportDiagnostic(Diagnostic.Create(_diagnosticDescriptor, methodDeclaration.GetLocation()));
        }
    }
}
