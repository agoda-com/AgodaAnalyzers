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
    public class AG0005TestMethodNamesMustFollowConvention : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AG0005";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0005Title), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));
        
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0005Title), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));
        
        private static readonly LocalizableString Description = DescriptionContentLoader.GetAnalyzerDescription(
            nameof(AG0005TestMethodNamesMustFollowConvention));

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

        // Test names must be in the format Xxxx_Yyyy or Xxxx_Yyyy_Zzzz 
        private static readonly Regex MatchValidTestName = new Regex("^[A-Z][a-zA-Z0-9]*_[A-Z0-9][a-zA-Z0-9]*(_[A-Z0-9][a-zA-Z0-9]*)?$");

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax) context.Node;

            // ensure public method
            if (!methodDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword)
                || methodDeclaration.IsKind(SyntaxKind.InterfaceDeclaration) 
                || methodDeclaration.IsKind(SyntaxKind.ExplicitInterfaceSpecifier))
            {
                return;
            }

            var hasNunitTestAttribute = context.SemanticModel
                .GetDeclaredSymbol(methodDeclaration)
                .GetAttributes()
                .Select(a => a.AttributeClass.BaseType.ToDisplayString())
                .Any(displayString => displayString == "NUnit.Framework.NUnitAttribute");
            if (!hasNunitTestAttribute) return;
                
            // ensure valid name
            var methodName = methodDeclaration.Identifier.ValueText;
            if (MatchValidTestName.IsMatch(methodName)) return;

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, methodDeclaration.GetLocation()));
        }
    }
}