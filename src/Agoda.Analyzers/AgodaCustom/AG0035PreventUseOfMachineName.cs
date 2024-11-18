using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0035PreventUseOfMachineName : PropertyInvocationAnalyzerBase
    {
        internal override Dictionary<string, string> Properties => new Dictionary<string, string>()
            { { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "10" } };

        public const string DIAGNOSTIC_ID = "AG0035";
        
        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0035Title), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));
        
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0035Description), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));
        
        protected override DiagnosticDescriptor Descriptor => new DiagnosticDescriptor(
            DIAGNOSTIC_ID, 
            Title, 
            MessageFormat, 
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Error, 
            AnalyzerConstants.EnabledByDefault, 
            DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0035PreventUseOfMachineName)),
            "https://agoda-com.github.io/standards-c-sharp/configuration/machine-name.html",
            WellKnownDiagnosticTags.EditAndContinue);

        protected override IEnumerable<InvocationRule> Rules => new[]
        {
            new BlacklistedInvocationRule("System.Environment", "MachineName"),
            new BlacklistedInvocationRule("System.Web.HttpServerUtilityBase", "MachineName"),
            new BlacklistedInvocationRule("System.Web.HttpServerUtility", "MachineName"),
        };
    }
}