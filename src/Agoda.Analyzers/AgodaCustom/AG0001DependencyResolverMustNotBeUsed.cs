﻿using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0001DependencyResolverMustNotBeUsed : PropertyInvocationAnalyzerBase
    {
        internal override Dictionary<string, string> Properties => new Dictionary<string, string>()
            { { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "10" } };

        public const string DIAGNOSTIC_ID = "AG0001";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0001Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0001MessageFormat),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description
            = DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0001DependencyResolverMustNotBeUsed));

        protected override DiagnosticDescriptor Descriptor => new DiagnosticDescriptor(
            DIAGNOSTIC_ID,
            Title,
            MessageFormat,
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Warning,
            AnalyzerConstants.EnabledByDefault,
            Description,
            $"https://github.com/agoda-com/AgodaAnalyzers/blob/master/doc/{DIAGNOSTIC_ID}.md",
            WellKnownDiagnosticTags.EditAndContinue);

        protected override IEnumerable<InvocationRule> Rules => new[]
        {
            new BlacklistedInvocationRule("System.Web.Mvc.DependencyResolver", "Current")
        };
    }
}