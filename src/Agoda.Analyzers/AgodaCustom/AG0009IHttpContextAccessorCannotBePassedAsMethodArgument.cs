using System;
using System.Collections.Immutable;
using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0009IHttpContextAccessorCannotBePassedAsMethodArgument : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0009";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0009Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0009MessageFormat),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description =
            DescriptionContentLoader.GetAnalyzerDescription(
                nameof(AG0009IHttpContextAccessorCannotBePassedAsMethodArgument));

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DIAGNOSTIC_ID, Title, MessageFormat, AnalyzerCategory.CustomQualityRules,
                DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, null,
                WellKnownDiagnosticTags.EditAndContinue);

        private static readonly Action<SyntaxNodeAnalysisContext> DependencyResolverUsageAction =
            HandleDependencyResolverUsage;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        private static void HandleDependencyResolverUsage(SyntaxNodeAnalysisContext context)
        {
            var methodParam = context.Node as ParameterSyntax;
            var paramType = (methodParam.Type as QualifiedNameSyntax)?.Right ?? methodParam.Type as SimpleNameSyntax;
            if (paramType == null)
                return;

            var paramTypeName = context.SemanticModel.GetTypeInfo(paramType).Type.ToDisplayString();
            if ("Microsoft.AspNetCore.Http.IHttpContextAccessor" == paramTypeName
                || "Microsoft.AspNetCore.Http.HttpContextAccessor" == paramTypeName)
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation()));
        }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(DependencyResolverUsageAction, SyntaxKind.Parameter);
        }
    }
}