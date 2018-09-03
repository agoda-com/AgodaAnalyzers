using System.Linq;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Agoda.Analyzers.Helpers;
using System.Threading.Tasks;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0025PreventUseOfTaskContinue : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AG0025";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0025Title), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));
        
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0025Title), 
            CustomRulesResources.ResourceManager, 
            typeof(CustomRulesResources));
        
        private static readonly LocalizableString Description = DescriptionContentLoader.GetAnalyzerDescription(
            nameof(AG0025PreventUseOfTaskContinue));

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
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            var identifier = GetMethodCallIdentifier(invocation);
            if (identifier == null)
            {
                return;
            }

            var methodCallSymbol = context.SemanticModel.GetSymbolInfo(identifier.Value.Parent).Symbol;
            if (methodCallSymbol == null)
            {
                return;
            }

            if (methodCallSymbol.ContainingType.ConstructedFrom.ToString() == "System.Threading.Tasks.Task" && methodCallSymbol.Name.StartsWith("Continue"))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, identifier.Value.GetLocation()));
            }
        }


        protected SyntaxToken? GetMethodCallIdentifier(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is IdentifierNameSyntax directMethodCall)
            {
                return directMethodCall.Identifier;
            }

            if (invocation.Expression is MemberAccessExpressionSyntax memberAccessCall)
            {
                return memberAccessCall.Name.Identifier;
            }

            return null;
        }
    
    }
}