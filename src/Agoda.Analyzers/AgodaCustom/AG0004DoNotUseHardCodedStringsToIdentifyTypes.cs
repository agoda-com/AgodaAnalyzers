using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Agoda.Analyzers.Helpers;
using System.Collections.Generic;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0004DoNotUseHardCodedStringsToIdentifyTypes : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0004";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0004Title), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));
        
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0004Title), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));
        
        private static readonly LocalizableString Description = DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0004DoNotUseHardCodedStringsToIdentifyTypes));

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DIAGNOSTIC_ID, 
            Title, 
            MessageFormat, 
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Warning, 
            AnalyzerConstants.EnabledByDefault, 
            Description, 
            "https://agoda-com.github.io/standards-c-sharp/reflection/hard-coded-strings.html", 
            WellKnownDiagnosticTags.EditAndContinue);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);
        
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        private static readonly BlacklistedInvocationRule GetTypeRule = new BlacklistedInvocationRule("System.Type", "GetType");

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var invocationExpressionSyntax = (InvocationExpressionSyntax) context.Node;
            if (!(context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol is IMethodSymbol methodSymbol))
            {
                return;
            }
            
            // Type.GetType("") - this method is completely banned as all its overloads take types as strings
            if (!GetTypeRule.Verify(methodSymbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation(), properties: _props.ToImmutableDictionary()));
                return;
            }

            // Activator.CreateInstance("", "") - we need to prevent the use only of the overloads that takes strings
            if (!(methodSymbol.ContainingType.ConstructedFrom.ToDisplayString() == "System.Activator" && methodSymbol.Name == "CreateInstance"))
            {
                return;
            }

            // ensure the overload called does not take a string literal as its first argument
            var firstParameter = methodSymbol.Parameters.FirstOrDefault();
            if (firstParameter == null)
            {
                return;
            }

            if (firstParameter.Type.Name.ToLower() == "string")
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation(), properties: _props.ToImmutableDictionary()));
            }
        }
        private static Dictionary<string, string> _props = new Dictionary<string, string>()
        {
            { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "10" }
        };
    }
}
