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
        private static readonly string[] AssertionIdentifiers = { "Assert", "Should" };

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
            var hasTestAttribute = methodDeclaration.AttributeLists
                .SelectMany(al => al.Attributes)
                .Select(a => a.Name as IdentifierNameSyntax)
                .Where(name => name != null)
                .Select(name => name.Identifier.ValueText)
                .Any(MatchTestAttributeName.IsMatch);

            if (!hasTestAttribute) return;
            
            // check if the method body invokes some kind of Assertion or not
            var hasAssertInvocation = methodDeclaration.Body.Statements
                .Where(statement => statement.IsKind(SyntaxKind.ExpressionStatement))
                .Select(statement => (statement as ExpressionStatementSyntax).Expression)
                .Where(expression => expression.IsKind(SyntaxKind.InvocationExpression))
                .Select(invocation => (invocation as InvocationExpressionSyntax).Expression)
                .Where(invocation => invocation.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                .Select(memberAccess => (memberAccess as MemberAccessExpressionSyntax).Expression)
                .Where(memberAccess => memberAccess.IsKind(SyntaxKind.IdentifierName))
                .Select(identifier => (identifier as IdentifierNameSyntax).Identifier)
                .Any(identifier => AssertionIdentifiers.Contains(identifier.Value));

            if (hasAssertInvocation) return;

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation()));
        }
    }
}