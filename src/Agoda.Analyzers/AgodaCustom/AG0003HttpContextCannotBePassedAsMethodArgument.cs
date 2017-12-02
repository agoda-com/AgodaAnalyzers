using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using StyleCop.Analyzers;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0003HttpContextCannotBePassedAsMethodArgument : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AG0003";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(CustomRulesResources.AG0003Title), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(CustomRulesResources.AG0003MessageFormat), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(CustomRulesResources.AG0003Description), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, AnalyzerCategory.CustomQualityRules,
                DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, null, WellKnownDiagnosticTags.EditAndContinue);

        private static readonly Action<SyntaxNodeAnalysisContext> DependencyResolverUsageAction = HandleDependencyResolverUsage;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        private static void HandleDependencyResolverUsage(SyntaxNodeAnalysisContext context)
        {
            var methodParam = context.Node as ParameterSyntax;

            var simpleType = (methodParam.Type as QualifiedNameSyntax)?.Right ?? methodParam.Type as SimpleNameSyntax;
            if (simpleType == null)
                return;

            if (context.SemanticModel.GetTypeInfo(simpleType).Type.ToDisplayString() == "System.Web.HttpContext")
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation()));
            }
        }


        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(DependencyResolverUsageAction, SyntaxKind.Parameter);
        }
    }
}