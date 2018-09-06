using Microsoft.CodeAnalysis;
using Agoda.Analyzers.Helpers;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Text.RegularExpressions;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0026EnsureOnlyCssSelectorIsUsedToFindElements : ForbiddenMethodAnalyzerBase
    {
        public const string DIAGNOSTIC_ID = "AG0026";

        private static readonly LocalizableResourceString _msg = new LocalizableResourceString(
                nameof(CustomRulesResources.AG0026Title), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));

        protected override DiagnosticDescriptor Descriptor => new DiagnosticDescriptor(
                DIAGNOSTIC_ID,
                _msg,
                _msg,
                AnalyzerCategory.CustomQualityRules,
                DiagnosticSeverity.Warning,
                AnalyzerConstants.EnabledByDefault,
                DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0026EnsureOnlyCssSelectorIsUsedToFindElements)),
                "https://github.agodadev.io/pages/standards-c-sharp/code-standards/gui-testing/css-selectors.html",
                WellKnownDiagnosticTags.EditAndContinue
            );

        protected override ImmutableArray<ForbiddenMethodRule> Rules =>
            ImmutableArray.Create(
                ForbiddenMethodRule.Create(
                    "OpenQA.Selenium.By",
                    new Regex("^(ClassName|Id|LinkText|Name|PartialLinkText|TagName|XPath)$")),
                ForbiddenMethodRule.Create(
                    "OpenQA.Selenium.Remote.RemoteWebDriver",
                    new Regex(
                            "^(FindElementByClassName|FindElementsByClassName" +
                            "|FindElementById|FindElementsById" +
                            "|FindElementByLinkText|FindElementsByLinkText" +
                            "|FindElementByName|FindElementsByName" +
                            "|FindElementByPartialLinkText|FindElementsByPartialLinkText" +
                            "|FindElementByTagName|FindElementsByTagName" +
                            "|FindElementByXPath|FindElementsByXPath)$"))
                );
    }
}
