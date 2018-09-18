using System;
using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0022DoNotExposeBothSyncAndAsyncVersionsOfMethods : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AG0022";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0022Title), CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0022MessageFormat), CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description =
            DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0022DoNotExposeBothSyncAndAsyncVersionsOfMethods));

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, AnalyzerCategory.CustomQualityRules,
                DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, null,
                WellKnownDiagnosticTags.EditAndContinue);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(AnalyzeNodeInAction, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeNodeInAction(SyntaxNodeAnalysisContext context)
        {
            var currentMethod = context.Node as MethodDeclarationSyntax;
            
            if (currentMethod == null &&
                !currentMethod.Parent.ChildNodes().Any()) return;

            IEnumerable<MethodDeclarationSyntax> slibingNodes = currentMethod.Parent.ChildNodes().OfType<MethodDeclarationSyntax>()
                .Where(n => n.Identifier.ValueText != currentMethod.Identifier.ValueText)
                .Select(n => n).ToList();

            var matchMethods = FindSyncMethodThatMatchAsyncMethod(currentMethod, slibingNodes);

            foreach(var matchMethod in matchMethods)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, matchMethod.GetLocation()));
            }            
        }        

        private static IEnumerable<MethodDeclarationSyntax> FindSyncMethodThatMatchAsyncMethod(MethodDeclarationSyntax node, IEnumerable<MethodDeclarationSyntax> slibingNodes)
        {
            var nodeMethodName = node.Identifier.ValueText;
            if (nodeMethodName.EndsWith("Async"))
            {
                var targetSyncMatchMethod = nodeMethodName.Remove(nodeMethodName.IndexOf("Async", StringComparison.Ordinal));
                return from s in slibingNodes
                       where s.Identifier.ValueText == targetSyncMatchMethod
                       select s;
            }
            else
            {
                var targetAsyncMatchMethod = nodeMethodName + "Async";
                return slibingNodes.Any(s => s.Identifier.ValueText == targetAsyncMatchMethod) ?
                    new List<MethodDeclarationSyntax> { node } :
                    new List<MethodDeclarationSyntax>();
            }     
        }
    }
}
