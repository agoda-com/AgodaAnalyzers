﻿using Microsoft.CodeAnalysis;
using Agoda.Analyzers.Helpers;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Text.RegularExpressions;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0026EnsureOnlyCssSelectorIsUsedToFindElements : ForbiddenMethodInvocationAnalyzerBase
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
            "https://github.agodadev.io/pages/standards-c-sharp/code-standards/gui-testing/css-selectors.html",
            WellKnownDiagnosticTags.EditAndContinue);

        protected override ImmutableArray<ForbiddenInvocationRule> Rules =>
            ImmutableArray.Create(
                ForbiddenInvocationRule.Create(
                    "OpenQA.Selenium.By",
                    new Regex("^(?!CssSelector).*$")),
                ForbiddenInvocationRule.Create(
                    "OpenQA.Selenium.Remote.RemoteWebDriver",
                    new Regex("^(FindElement[s]?((?!ByCssSelector)[B]+)(.+))$"))
                );
    }
}
