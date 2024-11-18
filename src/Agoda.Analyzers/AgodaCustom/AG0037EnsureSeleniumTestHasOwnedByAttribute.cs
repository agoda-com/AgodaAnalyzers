using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0037EnsureSeleniumTestHasOwnedByAttribute : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0037";
        
        private const string OWNED_BY_ATTRIBUTE_CLASS_NAME = "OwnedByAttribute";
        
        // Hmm, is there a better way of identifying selenium tests?
        private static readonly List<Regex> MatchNamespaces = new List<Regex>
        {
            new Regex(@"Selenium.*Test"),
            new Regex(@"Test.*Selenium"),
        };
        
        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0037Title), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));
        
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0037Description), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));
        
        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DIAGNOSTIC_ID, 
            Title, 
            MessageFormat, 
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Error, 
            AnalyzerConstants.EnabledByDefault, 
            DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0037EnsureSeleniumTestHasOwnedByAttribute)),
            null,
            WellKnownDiagnosticTags.EditAndContinue);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);
        
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var methodSyntax = (MethodDeclarationSyntax) context.Node;
            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodSyntax);

            // check the method is declared in the right namespace
            if (!MatchNamespaces.Any(r => r.IsMatch(methodSymbol.ContainingNamespace.ToString())))
            {
                return;
            }

            // ensure it's a test method
            if (!TestMethodHelpers.IsTestCase(methodSyntax, context))
            {
                return;
            }

            // check if the method has the attribute 
            if (methodSymbol.GetAttributes().Any(a => a.AttributeClass.Name == OWNED_BY_ATTRIBUTE_CLASS_NAME))
            {
                return;
            }
            
            // check if the class has the attribute
            var classHasAttribute = methodSyntax.Ancestors()
                .OfType<ClassDeclarationSyntax>()
                .SelectMany(classDeclarationSyntax => context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax).GetAttributes())
                .Any(a => a.AttributeClass.Name == OWNED_BY_ATTRIBUTE_CLASS_NAME);
            
            if (classHasAttribute)
            {
                return;
            }
            
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation(), properties: _props.ToImmutableDictionary()));
        }

        private static Dictionary<string, string> _props = new Dictionary<string, string>()
        {
            { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "10" }
        };
    }
}