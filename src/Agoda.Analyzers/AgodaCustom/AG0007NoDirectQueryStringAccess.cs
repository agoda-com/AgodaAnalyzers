using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0007NoDirectQueryStringAccess : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AG0007";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(CustomRulesResources.AG0007Title), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(CustomRulesResources.AG0007Title), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));
        private static readonly LocalizableString Description = DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0007NoDirectQueryStringAccess));

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, AnalyzerCategory.CustomQualityRules,
                DiagnosticSeverity.Error, AnalyzerConstants.EnabledByDefault, Description, null, WellKnownDiagnosticTags.EditAndContinue);

        private static readonly Action<SyntaxNodeAnalysisContext> NoDirectQueryStringAccess = HandleNoDirectQueryStringAccess;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        private static void HandleNoDirectQueryStringAccess(SyntaxNodeAnalysisContext context)
        {
            var identifier = context.Node as IdentifierNameSyntax;

            if (identifier?.Identifier.Text != "QueryString")
                return;

            if (context.SemanticModel.GetTypeInfo(identifier).Type.ToDisplayString() == "System.Collections.Specialized.NameValueCollection")
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation()));
            }
        }
        
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(NoDirectQueryStringAccess, SyntaxKind.IdentifierName);
        }
    }
}