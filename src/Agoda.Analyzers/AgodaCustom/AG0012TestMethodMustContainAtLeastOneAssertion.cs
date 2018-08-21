using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Agoda.Analyzers.Helpers;
using System.Linq;
using System.Text.RegularExpressions;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0012TestMethodMustContainAtLeastOneAssertion : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AG0012";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(CustomRulesResources.AG0012Title), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(CustomRulesResources.AG0012Title), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));
        private static readonly LocalizableString Description = DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0012TestMethodMustContainAtLeastOneAssertion));
        
        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, AnalyzerCategory.CustomQualityRules,
                DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, null, WellKnownDiagnosticTags.EditAndContinue);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        private static readonly Regex MatchTestAttributeName = new Regex("^Test");

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
        }
        
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = context.Node as MethodDeclarationSyntax;
            if (!methodDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword)
                || methodDeclaration.IsKind(SyntaxKind.InterfaceDeclaration)
                || methodDeclaration.IsKind(SyntaxKind.ExplicitInterfaceSpecifier))
            {
                return;
            }

            // check if it is a Test method with Test attribute or not
            if (!IsTestMethod(methodDeclaration)) return;

            // check if the method body invokes some kind of Assertion or not
            if (HasNUnitAssertion(methodDeclaration, context) || HasShouldlyAssertion(methodDeclaration, context)) return;

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation()));
        }

        private static bool IsTestMethod(MethodDeclarationSyntax methodDeclaration)
        {
            return methodDeclaration.AttributeLists
                .SelectMany(al => al.Attributes)
                .Select(a => a.Name as IdentifierNameSyntax)
                .Where(name => name != null)
                .Select(name => name.Identifier.ValueText)
                .Any(MatchTestAttributeName.IsMatch);
        }

        private static bool HasNUnitAssertion(MethodDeclarationSyntax methodDeclaration, SyntaxNodeAnalysisContext context)
        {
            return methodDeclaration.Body.Statements
                .OfType<ExpressionStatementSyntax>()
                .Select(ess => ess.Expression)
                .OfType<InvocationExpressionSyntax>()
                .Select(ies => ies.Expression)
                .OfType<MemberAccessExpressionSyntax>()
                .Select(mae => mae.Expression)
                .OfType<IdentifierNameSyntax>()
                .Select(ins => context.SemanticModel.GetSymbolInfo(ins).Symbol)
                .Any(symbol => symbol.ContainingNamespace.ToDisplayString() == "NUnit.Framework"
                            && symbol.ContainingModule.ToDisplayString() == "nunit.framework.dll"
                            && symbol.Name.StartsWith("Assert"));
        }

        private static bool HasShouldlyAssertion(MethodDeclarationSyntax methodDeclaration, SyntaxNodeAnalysisContext context)
        {
            return methodDeclaration.Body.Statements
                .OfType<ExpressionStatementSyntax>()
                .Select(ess => ess.Expression)
                .OfType<InvocationExpressionSyntax>()
                .Select(ies => ies.Expression)
                .OfType<MemberAccessExpressionSyntax>()
                .Select(mae => mae.Name)
                .OfType<IdentifierNameSyntax>()
                .Select(ins => context.SemanticModel.GetSymbolInfo(ins).Symbol)
                .Any(symbol => symbol.ContainingNamespace.ToDisplayString() == "Shouldly"
                            && symbol.ContainingModule.ToDisplayString() == "Shouldly.dll"
                            && symbol.ContainingType.ToDisplayString() == "Shouldly.ShouldBeTestExtensions"
                            && symbol.Name.StartsWith("Should"));
        }
    }
}