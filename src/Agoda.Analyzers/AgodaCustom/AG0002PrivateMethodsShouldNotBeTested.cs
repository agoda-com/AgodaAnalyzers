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
        public const string DiagnosticId = "AG0002";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(CustomRulesResources.AG0002Title), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(CustomRulesResources.AG0002MessageFormat), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(CustomRulesResources.AG0002Description), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));
        private const string Category = "Usage";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category, DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(Rule); }
        }

        public override void Initialize(AnalysisContext context)
        {
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

            context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
        }
    }
}