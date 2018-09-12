using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0011NoDirectQueryStringAccess : ForbiddenPropertyInvocationAnalyzerBase
    {
        public const string DIAGNOSTIC_ID = "AG0011";
        
        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0011Title), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));
        
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0011Title), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));
        
        protected override DiagnosticDescriptor Descriptor => new DiagnosticDescriptor(
            DIAGNOSTIC_ID, 
            Title, 
            MessageFormat, 
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Error, 
            AnalyzerConstants.EnabledByDefault, 
            DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0011NoDirectQueryStringAccess)),
            null, 
            WellKnownDiagnosticTags.EditAndContinue);

        protected override ImmutableArray<ForbiddenInvocationRule> Rules =>
            ImmutableArray.Create(ForbiddenInvocationRule.Create("System.Web.HttpRequest", new Regex("^QueryString$")));


    }
}