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
    public class AG0023PreventUseOfThreadSleep : ForbiddenMethodInvocationAnalyzerBase
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
            "https://github.agodadev.io/pages/standards-c-sharp/code-standards/async/avoid-blocking.html", 
            WellKnownDiagnosticTags.EditAndContinue);


        protected override ImmutableArray<ForbiddenInvocationRule> Rules => 
            ImmutableArray.Create(ForbiddenInvocationRule.Create("System.Threading.Thread", new Regex("^Sleep")));
    }
}
