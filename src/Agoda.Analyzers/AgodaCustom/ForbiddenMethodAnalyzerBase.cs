using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace Agoda.Analyzers.AgodaCustom
{
    public abstract class ForbiddenMethodAnalyzerBase : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);
        protected abstract DiagnosticDescriptor Descriptor { get; }
        protected abstract ImmutableArray<ForbiddenMethodRule> Rules { get; }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var invocationExpressionSyntax = (InvocationExpressionSyntax)context.Node;
            var methodSymbol = (IMethodSymbol)context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol;

            Rules
                .Where(x => Filter(methodSymbol, x))
                .ToList()
                .ForEach(x => context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocationExpressionSyntax.GetLocation())));
        }

        private static bool Filter(IMethodSymbol methodSymbol, ForbiddenMethodRule rule)
                => methodSymbol.ContainingType.ConstructedFrom.ToString() == rule.NamespaceAndType
                        && rule.MethodNameRegex.IsMatch(methodSymbol.Name);
    }

    public class ForbiddenMethodRule
    {
        public string NamespaceAndType { get; }

        public Regex MethodNameRegex { get; }

        private ForbiddenMethodRule(string namespaceAndType, Regex methodNameRegex)
        {
            NamespaceAndType = namespaceAndType;
            MethodNameRegex = methodNameRegex;
        }

        public static ForbiddenMethodRule Create(string namespaceAndType, Regex methodNameRegex)
            => new ForbiddenMethodRule(namespaceAndType, methodNameRegex);
    }
}
