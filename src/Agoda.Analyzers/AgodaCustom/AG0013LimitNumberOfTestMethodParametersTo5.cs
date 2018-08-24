using System.Linq;
using Microsoft.CodeAnalysis;
using Agoda.Analyzers.Helpers;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0013LimitNumberOfTestMethodParametersTo5 : DiagnosticAnalyzer
    {
        private const int MAXIMUM_TESTCASE = 5;
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
            var methodDeclaration = (MethodDeclarationSyntax) context.Node;

            if (!MethodHelper.IsTestCase(methodDeclaration, context)) { return; }
                
            if(!IsTestCaseAttribtuionMoreThanLimit(methodDeclaration)) { return; }

            context.ReportDiagnostic(Diagnostic.Create(_diagnosticDescriptor, methodDeclaration.GetLocation()));
        }

        private bool IsTestCaseAttribtuionMoreThanLimit(MethodDeclarationSyntax method)
        {
            if (!method.AttributeLists.Any()) { return false; }

            var testCaseCount = method.
                                AttributeLists.
                                Select(x => GetNumberOfTestCaseFromAttribute(x.ToString())).
                                Sum();

            return testCaseCount > MAXIMUM_TESTCASE;
        }

        private int GetNumberOfTestCaseFromAttribute(string attributeSyntax) => Regex.Matches(attributeSyntax, "TestCase").Count;
    }
}
