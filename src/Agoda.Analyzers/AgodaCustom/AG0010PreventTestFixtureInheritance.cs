using System.Linq;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Agoda.Analyzers.Helpers;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0010PreventTestFixtureInheritance : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AG0010";
        private static readonly Regex MatchTestAttributeName = new Regex("^TestFixture$");

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0010Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0010Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description = DescriptionContentLoader.GetAnalyzerDescription(
            nameof(AG0010PreventTestFixtureInheritance));

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Warning,
            AnalyzerConstants.EnabledByDefault,
            Description,
            null,
            WellKnownDiagnosticTags.EditAndContinue);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;

            var hasTestFixtureAttribute = classDeclaration.AttributeLists
                .SelectMany(al => al.Attributes)
                .Select(a => a.Name as IdentifierNameSyntax)
                .Where(name => name != null)
                .Select(name => name.Identifier.ValueText)
                .Any(MatchTestAttributeName.IsMatch);

            if (!hasTestFixtureAttribute) return;

            if (classDeclaration.BaseList == null) return;

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, classDeclaration.GetLocation()));
        }
    }
}