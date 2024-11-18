using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0033PreventUseOfTaskResult : PropertyInvocationAnalyzerBase
    {
        internal override Dictionary<string, string> Properties => new Dictionary<string, string>()
            { { Const.KEY_TECH_DEBT_IN_MINUTES, "10" } };

        public const string DIAGNOSTIC_ID = "AG0033";
        
        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0033Title), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));
        
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0033Title), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));
        
        protected override DiagnosticDescriptor Descriptor => new DiagnosticDescriptor(
            DIAGNOSTIC_ID, 
            Title, 
            MessageFormat, 
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Error, 
            AnalyzerConstants.EnabledByDefault, 
            DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0033PreventUseOfTaskResult)),
            "https://agoda-com.github.io/standards-c-sharp/async/await-task-result.html", 
            WellKnownDiagnosticTags.EditAndContinue);

        protected override IEnumerable<InvocationRule> Rules => new[]
        {
            new BlacklistedInvocationRule("System.Threading.Tasks.Task", "Result")
        };
    }
}