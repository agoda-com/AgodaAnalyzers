using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Agoda.Analyzers.Helpers;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Text.RegularExpressions;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0026EnsureOnlyCssSelectorIsUsedToFindElements : PermittedMethodInvocationAnalyzerBase
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
            "https://agoda-com.github.io/standards-c-sharp/gui-testing/css-selectors.html",
            WellKnownDiagnosticTags.EditAndContinue);
        
        protected override IEnumerable<PermittedInvocationRule> Rules => TestMethodHelpers.PermittedSeleniumAccessors;
    }
}
