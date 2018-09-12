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
    class AG0022DoNotExposeBothSyncAndAsyncVersionsOfMethods : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AG0022";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(CustomRulesResources.AG0022Title), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(CustomRulesResources.AG0022MessageFormat), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));
        private static readonly LocalizableString Description = DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0022DoNotExposeBothSyncAndAsyncVersionsOfMethods));
        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, AnalyzerCategory.CustomQualityRules,
                DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, null, WellKnownDiagnosticTags.EditAndContinue);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var currentMethodDeclaration = (MethodDeclarationSyntax)context.Node;
            var currentMethodName = currentMethodDeclaration.Identifier.ValueText;

            if (!context.Node.Parent.ChildNodes().Any() || "Async".Equals(currentMethodName)) return;

            IEnumerable<string> methodNames = context.Node.Parent.ChildNodes()
                .Where(node => ((MethodDeclarationSyntax)node).Identifier.ValueText != currentMethodName)
                .Select(node => ((MethodDeclarationSyntax)node).Identifier.ValueText).ToList();

            // ensure valid name
            if (!ExistsBothSyncAndAsyncVersionsOfMethods(currentMethodName, methodNames)) return;

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, currentMethodDeclaration.GetLocation()));
        }

        private bool ExistsBothSyncAndAsyncVersionsOfMethods(string currentMethodName, IEnumerable<string> methodNames)
        {
            var compareToMethodName = currentMethodName.EndsWith("Async") ?
                currentMethodName.Remove(currentMethodName.Length - 6, currentMethodName.Length - 1) :
                currentMethodName + "Async";

            return methodNames.Any(methodName => methodName.Equals(compareToMethodName));
        }
    }
}
