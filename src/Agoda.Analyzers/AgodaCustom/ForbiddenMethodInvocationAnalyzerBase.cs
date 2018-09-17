using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace Agoda.Analyzers.AgodaCustom
{
    /// <summary>
    /// Base class from which we can forbid the invocation of certain methods simply by defining their
    /// namespace, type and name. 
    /// </summary>
    public abstract class ForbiddenMethodInvocationAnalyzerBase : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);
        protected abstract DiagnosticDescriptor Descriptor { get; }
        protected abstract ImmutableArray<ForbiddenInvocationRule> Rules { get; }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocationExpressionSyntax = (InvocationExpressionSyntax)context.Node;
            if (!(context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol is IMethodSymbol methodSymbol)) return;

            Rules
                .Where(rule => methodSymbol.ContainingType.ConstructedFrom.ToString() == rule.NamespaceAndType)
                .Where(rule => rule.ForbiddenIdentifierNameRegex.IsMatch(methodSymbol.Name))
                .ToList()
                .ForEach(_ => context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation())));
        }
    }
}
