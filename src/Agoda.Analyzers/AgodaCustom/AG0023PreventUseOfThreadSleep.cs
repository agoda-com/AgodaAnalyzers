using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;

namespace Agoda.Analyzers.AgodaCustom
{
    [Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0023PreventUseOfThreadSleep : PermittedMethodInvocationAnalyzerBase
    {
        public const string DIAGNOSTIC_ID = "AG0023";

        private static readonly LocalizableString Title = new LocalizableResourceString(
           nameof(CustomRulesResources.AG0023Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));


        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
           nameof(CustomRulesResources.AG0023Title),
           CustomRulesResources.ResourceManager,
           typeof(CustomRulesResources));


        protected override DiagnosticDescriptor Descriptor => new DiagnosticDescriptor(
            DIAGNOSTIC_ID,
            Title,
            MessageFormat,
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Warning,
            AnalyzerConstants.EnabledByDefault,
            nameof(AG0023PreventUseOfThreadSleep),
            "https://agoda-com.github.io/standards-c-sharp/async/avoid-blocking.html", 
            WellKnownDiagnosticTags.EditAndContinue);


        protected override IEnumerable<PermittedInvocationRule> Rules => new[]
        {
            new BlacklistedInvocationRule("System.Threading.Thread", "Sleep"),
        };
    }
}
