using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0026EnsureOnlyCssSelectorIsUsedToFindElements : MethodInvocationAnalyzerBase
    {
        public const string DIAGNOSTIC_ID = "AG0026";

        private static readonly LocalizableResourceString Msg = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0026Title), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));

        protected override DiagnosticDescriptor Descriptor => new DiagnosticDescriptor(
            DIAGNOSTIC_ID,
            Msg,
            Msg,
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Warning,
            AnalyzerConstants.EnabledByDefault,
            DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0026EnsureOnlyCssSelectorIsUsedToFindElements)),
            $"https://github.com/agoda-com/AgodaAnalyzers/blob/master/doc/{DIAGNOSTIC_ID}.md", 
            WellKnownDiagnosticTags.EditAndContinue);
        
        protected override IEnumerable<InvocationRule> Rules => TestMethodHelpers.PermittedSeleniumAccessors;
    }
}
