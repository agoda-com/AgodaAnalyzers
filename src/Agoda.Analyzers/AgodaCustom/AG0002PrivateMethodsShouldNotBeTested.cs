using System.Collections.Generic;
using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0002PrivateMethodsShouldNotBeTested : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0002";
        
        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0002Title), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));
        
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0002MessageFormat), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));
        
        private static readonly LocalizableString Description 
            = DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0002PrivateMethodsShouldNotBeTested));

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DIAGNOSTIC_ID,
            Title,
            MessageFormat,
            AnalyzerCategory.CustomQualityRules, 
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description,
            $"https://github.com/agoda-com/AgodaAnalyzers/blob/master/doc/{DIAGNOSTIC_ID}.md"
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax) context.Node;

            if (!methodDeclaration.Modifiers.Any(SyntaxKind.InternalKeyword))
            {
                return;
            }

            if (methodDeclaration.IsKind(SyntaxKind.InterfaceDeclaration) || methodDeclaration.IsKind(SyntaxKind.ExplicitInterfaceSpecifier))
            {
                return;
            }

            /*
             var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration) as IMethodSymbol;

             Currently Roslyn analyzers not support solution based analysis ( because of some kind of performance reason )
             So right not not possible to filter out unreferenced errors in the specific project.

             Links : http://stackoverflow.com/questions/32566756/visual-studio-code-analyzer-finding-types-with-zero-references
                   : http://stackoverflow.com/questions/23203206/roslyn-current-workspace-in-diagnostic-with-code-fix-project
             */

            //var references = SymbolFinder.FindReferencesAsync(methodSymbol, null).Result;
            //if (references.Count() > 1)

            context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(),properties: _props.ToImmutableDictionary()));
        }

        private static Dictionary<string, string> _props = new Dictionary<string, string>()
        {
            { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "10" }
        };
    }
}
