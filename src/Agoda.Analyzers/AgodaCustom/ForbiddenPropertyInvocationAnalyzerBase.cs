using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using Agoda.Analyzers.Helpers;

namespace Agoda.Analyzers.AgodaCustom
{
    /// <summary>
    /// Base class from which we can forbid the invocation of certain properties simply by defining their namespace, type and name. 
    /// </summary>
    public abstract class ForbiddenPropertyInvocationAnalyzerBase : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);
        protected abstract DiagnosticDescriptor Descriptor { get; }
        protected abstract IEnumerable<PermittedInvocationRule> Rules { get; }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeIdentifier, SyntaxKind.IdentifierName);
        }

        private void AnalyzeIdentifier(SyntaxNodeAnalysisContext context)
        {
            var identifierSyntax = (IdentifierNameSyntax) context.Node;
            var memberAccess = identifierSyntax?.Identifier.Parent?.Parent as MemberAccessExpressionSyntax;
            if (memberAccess == null) return;

            var memberType = context.SemanticModel.GetTypeInfo(memberAccess.Expression);
            var containingTypeName = memberType.Type?.ToDisplayString();

            if (Rules.Any(rule => !rule.Verify(containingTypeName, identifierSyntax.Identifier.Text)))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation()));
            }
        }
    }
}
