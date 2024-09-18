using System.Collections.Generic;
using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0040DependencyResolverMustNotBeUsed : PropertyInvocationAnalyzerBase
    {
        public const string DIAGNOSTIC_ID = "AG0040";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0040Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0040MessageFormat),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description
            = DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0040DependencyResolverMustNotBeUsed));

        protected override DiagnosticDescriptor Descriptor => new DiagnosticDescriptor(
            DIAGNOSTIC_ID,
            Title,
            MessageFormat,
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Warning,
            AnalyzerConstants.EnabledByDefault,
            Description,
            "https://playwright.dev/dotnet/docs/api/class-page#page-go-back",
            WellKnownDiagnosticTags.EditAndContinue);

        protected override IEnumerable<InvocationRule> Rules => new[]
        {
            new BlacklistedInvocationRule("Microsoft.Playwright.WaitUntilState", "NetworkIdle")
        };
    }
}