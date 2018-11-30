using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0038PreventUseOfRegionPreprocessorDirective : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0038";
        
        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0038Title), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));
        
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0038Title), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));
        
        private static readonly LocalizableString Description 
            = DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0038PreventUseOfRegionPreprocessorDirective));

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DIAGNOSTIC_ID, 
            Title, 
            MessageFormat, 
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Warning, 
            AnalyzerConstants.EnabledByDefault, 
            Description, 
            "https://agoda-com.github.io/standards-c-sharp/code-style/regions.html", 
            WellKnownDiagnosticTags.EditAndContinue);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.RegionDirectiveTrivia);
        }
        
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation()));
        }
    }
}