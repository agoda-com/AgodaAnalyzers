using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0030PreventUseOfDynamics : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0030";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0030Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0030Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description =
            DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0030PreventUseOfDynamics));

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

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeVariableDeclaration, SyntaxKind.VariableDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeGenericName, SyntaxKind.GenericName);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        private void AnalyzeVariableDeclaration(SyntaxNodeAnalysisContext context)
        {
            var variableDeclaration = (VariableDeclarationSyntax)context.Node;
            if (ValidateReturnType(variableDeclaration.Type)) return;
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, variableDeclaration.GetLocation(), properties: _props.ToImmutableDictionary()));
        }

        private void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;
            if (ValidateReturnType(methodDeclaration.ReturnType)) return;
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, methodDeclaration.GetLocation(), properties: _props.ToImmutableDictionary()));
        }

        private void AnalyzeGenericName(SyntaxNodeAnalysisContext context)
        {
            var genericName = (GenericNameSyntax)context.Node;
            if (ValidateGenericType(genericName)) return;
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, genericName.GetLocation(), properties: _props.ToImmutableDictionary()));
        }

        private bool ValidateReturnType(TypeSyntax returnTypeSyntax)
        {
            var returnType = returnTypeSyntax.GetText().ToString().Trim();
            return returnType != "dynamic";
        }

        private bool ValidateGenericType(GenericNameSyntax genericName)
        {
            return genericName.TypeArgumentList.Arguments.All(argument => argument.GetText().ToString() != "dynamic");
        }

        private static Dictionary<string, string> _props = new Dictionary<string, string>()
        {
            { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "30" }
        };
    }
}