﻿using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0019PreventUseOfInternalsVisibleToAttribute : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0019";
        private readonly DiagnosticDescriptor _diagnosticDescriptor;

        public AG0019PreventUseOfInternalsVisibleToAttribute()
        {
            var info = new LocalizableResourceString(nameof(CustomRulesResources.AG0019Title), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));

            _diagnosticDescriptor = new DiagnosticDescriptor(
                DIAGNOSTIC_ID,
                info,
                info,
                AnalyzerCategory.MaintainabilityRules,
                DiagnosticSeverity.Error,
                AnalyzerConstants.EnabledByDefault,
                DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0019PreventUseOfInternalsVisibleToAttribute)),
                "https://github.agodadev.io/pages/standards-c-sharp/code-standards/unit-testing/only-test-the-public-interface.html",
                WellKnownDiagnosticTags.EditAndContinue);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_diagnosticDescriptor);

        public override void Initialize(AnalysisContext context) => context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.AttributeList);

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var attributeNode = (AttributeListSyntax)context.Node;

            if (attributeNode.Target?.Identifier.Text != "assembly") return;

            foreach (var attribute in attributeNode.Attributes)
            {
                if (attribute.Name is IdentifierNameSyntax name && name.Identifier.Text == "InternalsVisibleTo")
                {
                    context.ReportDiagnostic(Diagnostic.Create(_diagnosticDescriptor, attribute.GetLocation()));
                }
            }
        }
    }
}
