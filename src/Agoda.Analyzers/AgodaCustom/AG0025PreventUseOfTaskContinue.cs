﻿using System.Linq;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Agoda.Analyzers.Helpers;
using System.Threading.Tasks;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0025PreventUseOfTaskContinue : ForbiddenMethodInvocationAnalyzerBase
    {
        public const string DIAGNOSTIC_ID = "AG0025";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0025Title), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));
        
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0025Title), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));
        
        private static readonly LocalizableString Description = DescriptionContentLoader.GetAnalyzerDescription(
            nameof(AG0025PreventUseOfTaskContinue));
        
        protected override DiagnosticDescriptor Descriptor => new DiagnosticDescriptor(
            DIAGNOSTIC_ID, 
            Title, 
            MessageFormat, 
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Warning, 
            AnalyzerConstants.EnabledByDefault, 
            Description, 
            null, 
            WellKnownDiagnosticTags.EditAndContinue);

        protected override ImmutableArray<ForbiddenInvocationRule> Rules =>
            ImmutableArray.Create(ForbiddenInvocationRule.Create("System.Threading.Tasks.Task", new Regex("^Continue")));


    }
}