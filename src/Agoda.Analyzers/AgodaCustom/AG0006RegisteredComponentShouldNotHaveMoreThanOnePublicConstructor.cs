using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0006RegisteredComponentShouldNotHaveMoreThanOnePublicConstructor : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AG0006";
        private const string PUBLIC = "public";
        private static readonly Regex MatchTestAttributeName = new Regex("^Register");
        
        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0006Title), CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0006Title), CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description =
            DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0006RegisteredComponentShouldNotHaveMoreThanOnePublicConstructor));

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, AnalyzerCategory.CustomQualityRules,
                DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, null,
                WellKnownDiagnosticTags.EditAndContinue);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax) context.Node;
            
            var hasRegisterAttribute = classDeclaration.AttributeLists
                .SelectMany(al => al.Attributes)
                .Select(a => a.Name as IdentifierNameSyntax)
                .Where(name => name != null)
                .Select(name => name.Identifier.ValueText)
                .Any(MatchTestAttributeName.IsMatch);

            if (!hasRegisterAttribute) return;
            
            var constructors = classDeclaration.Members.ToList().FindAll(a => a is ConstructorDeclarationSyntax);
            var publicConstructors = constructors.FindAll(c =>
                ((ConstructorDeclarationSyntax) c).Modifiers.Any(t => t.Text.ToLower().Equals(PUBLIC)));
            
            if (publicConstructors.Count < 2)
                return;
            
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, classDeclaration.GetLocation()));
        }
    }
}