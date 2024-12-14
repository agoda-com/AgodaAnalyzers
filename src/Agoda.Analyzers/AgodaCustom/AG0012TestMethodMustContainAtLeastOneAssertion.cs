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
    public class AG0012TestMethodMustContainAtLeastOneAssertion : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0012";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0012Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0012Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description
            = DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0012TestMethodMustContainAtLeastOneAssertion));

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

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        private static readonly AssertLibraryInfo[] AssertionLibraryList =
        {
            new AssertLibraryInfo("NUnit.Framework", "nunit.framework.dll", "Assert"),
            new AssertLibraryInfo("Shouldly", "Shouldly.dll", "Should", "Shouldly.Should"),
            //FluentAssertions
        };

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = context.Node as MethodDeclarationSyntax;

            if (!TestMethodHelpers.IsTestCase(methodDeclaration, context)) return;

            // check if the method body invokes some kind of Assertion or not
            if (HasInvokedAssertStaticMethod(methodDeclaration, context) ||
                HasInvokedAssertExtensionMethod(methodDeclaration, context))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation(), properties: _props.ToImmutableDictionary()));
        }

        private static bool HasInvokedAssertStaticMethod(MethodDeclarationSyntax methodDeclaration, SyntaxNodeAnalysisContext context)
        {
            return methodDeclaration.Body.Statements
                .SelectMany(s => s.DescendantNodesAndSelf())
                .OfType<InvocationExpressionSyntax>()
                .Select(ies => ies.Expression)
                .OfType<MemberAccessExpressionSyntax>()
                .Select(mae => mae.Expression)
                .OfType<IdentifierNameSyntax>()
                .Select(ins => context.SemanticModel.GetSymbolInfo(ins).Symbol)
                .Any(symbol => AssertionLibraryList.Any(lib
                    => symbol.ContainingNamespace.ToDisplayString() == lib.Namespace
                    && symbol.ContainingModule.ToDisplayString() == lib.Module
                    && symbol.Name == lib.Name));
        }

        private static bool HasInvokedAssertExtensionMethod(MethodDeclarationSyntax methodDeclaration, SyntaxNodeAnalysisContext context)
        {
            var i = methodDeclaration.Body.Statements
                .SelectMany(s => s.DescendantNodesAndSelf())
                .OfType<InvocationExpressionSyntax>()
                .Select(ies => ies.Expression)
                .OfType<MemberAccessExpressionSyntax>();
            var z = i.Select(mae => mae.Name)
                .OfType<IdentifierNameSyntax>();
            foreach (var identifierNameSyntax in z)
            {
                var t = context.SemanticModel.GetSymbolInfo(identifierNameSyntax);
                var u = context.SemanticModel.GetDiagnostics();
                var r = context.SemanticModel.GetDeclaredSymbol(identifierNameSyntax);
            }
            var y = z.Select(ins => context.SemanticModel.GetSymbolInfo(ins).Symbol)
            .Any(symbol => AssertionLibraryList
                    .Where(lib => lib.HasExtenstionMethods).Any(lib
                    => symbol.ContainingNamespace.ToDisplayString() == lib.Namespace
                    && symbol.ContainingModule.ToDisplayString() == lib.Module
                    && symbol.ContainingType.ToDisplayString().StartsWith(lib.Type)
                    && symbol.Name.StartsWith(lib.Name)));
            return y;
        }

        private class AssertLibraryInfo
        {
            public string Namespace { get; }
            public string Module { get; }
            public string Name { get; }
            public string Type { get; }
            public bool HasExtenstionMethods { get; }

            public AssertLibraryInfo(string namespaceTitle, string module, string name, string type = null)
            {
                Namespace = namespaceTitle;
                Module = module;
                Name = name;
                Type = type;
                HasExtenstionMethods = type != null;
            }
        }
        private static Dictionary<string, string> _props = new Dictionary<string, string>()
        {
            { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "10" }
        };
    }
}