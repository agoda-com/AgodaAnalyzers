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
    public class AG0032PreventUseOfBlockingTaskMethods : PropertyInvocationAnalyzerBase
    {
        internal override Dictionary<string, string> Properties => new Dictionary<string, string>()
            { { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "10" } };

        public const string DIAGNOSTIC_ID = "AG0032";
        
        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0032Title), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));
        
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0032Title), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));
        
        protected override DiagnosticDescriptor Descriptor => new DiagnosticDescriptor(
            DIAGNOSTIC_ID, 
            Title, 
            MessageFormat, 
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Error, 
            AnalyzerConstants.EnabledByDefault, 
            DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0032PreventUseOfBlockingTaskMethods)),
            "https://agoda-com.github.io/standards-c-sharp/async/never-task-wait.html", 
            WellKnownDiagnosticTags.EditAndContinue);

        protected override IEnumerable<InvocationRule> Rules => new[]
        {
            new BlacklistedInvocationRule("System.Threading.Tasks.Task",
                new Regex("^Wait"),
                new Regex("^GetAwaiter$"))
        };
    }
}