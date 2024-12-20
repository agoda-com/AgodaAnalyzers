﻿using System.Linq;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Agoda.Analyzers.Helpers;
using System.Collections.Generic;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0005TestMethodNamesMustFollowConvention : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0005";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0005Title), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));
        
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0005Title), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));
        
        private static readonly LocalizableString Description = DescriptionContentLoader.GetAnalyzerDescription(
            nameof(AG0005TestMethodNamesMustFollowConvention));

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DIAGNOSTIC_ID, 
            Title, 
            MessageFormat, 
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Warning, 
            AnalyzerConstants.EnabledByDefault, 
            Description,
            $"https://github.com/agoda-com/AgodaAnalyzers/blob/master/doc/{DIAGNOSTIC_ID}.md", 
            WellKnownDiagnosticTags.EditAndContinue);

        // Test names must be in the format Xxxx_Yyyy or Xxxx_Yyyy_Zzzz 
        private static readonly Regex MatchValidTestName = new Regex("^[A-Z][a-zA-Z0-9]*_[A-Z0-9][a-zA-Z0-9]*(_[A-Z0-9][a-zA-Z0-9]*)?$");

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax) context.Node;

            // ensure is test case 
            if (!TestMethodHelpers.IsTestCase(methodDeclaration, context))
            {
                return;
            }
            
            // ensure name is valid
            var methodName = methodDeclaration.Identifier.ValueText;
            if (MatchValidTestName.IsMatch(methodName))
            {
                return;
            }

            // report error at position of method name
            var methodNameToken = methodDeclaration.ChildTokens().First(t => t.IsKind(SyntaxKind.IdentifierToken));
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, methodNameToken.GetLocation(), properties: _props.ToImmutableDictionary()));
        }
        private static Dictionary<string, string> _props = new Dictionary<string, string>()
        {
            { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "10" }
        };
    }
}