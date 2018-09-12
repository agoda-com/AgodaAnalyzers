using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Agoda.Analyzers.AgodaCustom
{
    /// <summary>
    /// Base class from which we can forbid the invocation of certain properties simply by defining their namespace, type and name. 
    /// </summary>
    public abstract class ForbiddenPropertyInvocationAnalyzerBase : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);
        protected abstract DiagnosticDescriptor Descriptor { get; }
        protected abstract ImmutableArray<ForbiddenInvocationRule> Rules { get; }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIdentifier, SyntaxKind.IdentifierName);
        }

        private void AnalyzeIdentifier(SyntaxNodeAnalysisContext context)
        {
            var identifier = (IdentifierNameSyntax) context.Node;
            var memberAccess = identifier?.Identifier.Parent?.Parent as MemberAccessExpressionSyntax;
            if (memberAccess == null) return;

            var memberType = context.SemanticModel.GetTypeInfo(memberAccess.Expression);
            
            Rules
                .Where(rule => rule.NamespaceAndType == memberType.Type?.ToDisplayString())
                .Where(rule => rule.ForbiddenIdentifierNameRegex.IsMatch(identifier.Identifier.Text))
                .ToList()
                .ForEach(_ => context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation())));
        }
    }
}
