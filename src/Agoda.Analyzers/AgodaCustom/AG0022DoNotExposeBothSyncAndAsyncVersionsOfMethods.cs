using System;
using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0022DoNotExposeBothSyncAndAsyncVersionsOfMethods : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0022";
        
        private static readonly Regex MatchAsyncMethod = new Regex("Async$"); 

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0022Title), 
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0022MessageFormat), 
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description =
            DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0022DoNotExposeBothSyncAndAsyncVersionsOfMethods));

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DIAGNOSTIC_ID, 
            Title, 
            MessageFormat, 
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Warning, 
            AnalyzerConstants.EnabledByDefault, 
            Description, 
            null,
            WellKnownDiagnosticTags.EditAndContinue);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is MethodDeclarationSyntax methodDeclarationSyntax))
            {
                return;
            }
            
            // ensure public method or interface specification
            if (!(methodDeclarationSyntax.Modifiers.Any(SyntaxKind.PublicKeyword)
                || methodDeclarationSyntax.Parent.IsKind(SyntaxKind.InterfaceDeclaration)))
            {
                return;
            }
            
            // ensure method is async in intent
            var methodDeclaration = (IMethodSymbol) context.SemanticModel.GetDeclaredSymbol(context.Node);
            if (!AsyncHelpers.IsAsyncIntent(methodDeclaration))
            {
                return;
            }

            // find other methods with matching names
            var targetName = MatchAsyncMethod.Replace(methodDeclarationSyntax.Identifier.Text, "");
            var matchingMethods = methodDeclarationSyntax.Parent.ChildNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(methodSyntax => methodSyntax != context.Node)
                .Where(methodSyntax => methodSyntax.Identifier.Text == targetName);
            
            // if any of these methods are sync then report
            foreach (var methodSyntax in matchingMethods)
            {
                var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodSyntax);
                if (!AsyncHelpers.IsAsyncIntent(methodSymbol))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, methodSyntax.GetLocation()));    
                }
            }
                         
        }        
    }
}
