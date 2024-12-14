using System.Linq;
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
    public class AG0010PreventTestFixtureInheritance : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0010";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0010Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0010Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description = DescriptionContentLoader.GetAnalyzerDescription(
            nameof(AG0010PreventTestFixtureInheritance));

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DIAGNOSTIC_ID,
            Title,
            MessageFormat,
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Warning,
            AnalyzerConstants.EnabledByDefault,
            Description,
            $"https://github.com/agoda-com/AgodaAnalyzers/blob/master/doc/{DIAGNOSTIC_ID}.md", 
            WellKnownDiagnosticTags.EditAndContinue);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax) context.Node;
            if (classDeclaration.BaseList == null) return;

            var hasTestMethod = classDeclaration
                .ChildNodes()
                .OfType<MethodDeclarationSyntax>()
                .Any(x => TestMethodHelpers.IsTestCase(x, context));
            if (!hasTestMethod) return;

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, classDeclaration.GetLocation(), properties: _props.ToImmutableDictionary()));
        }
        private static Dictionary<string, string> _props = new Dictionary<string, string>()
        {
            { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "10" }
        };
    }
}