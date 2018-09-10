using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            IEnumerable<MethodDeclarationSyntax> methods = context.Node.
            .DescendentNodes()
            .OfType<MethodDeclarationSyntax>().ToList();

            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            if (!MethodHelper.IsTestCase(methodDeclaration, context)) return;

            // ensure valid name
            var methodName = methodDeclaration.Identifier.ValueText;
            if (MatchValidTestName.IsMatch(methodName)) return;

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, methodDeclaration.GetLocation()));
        }
    }
}
