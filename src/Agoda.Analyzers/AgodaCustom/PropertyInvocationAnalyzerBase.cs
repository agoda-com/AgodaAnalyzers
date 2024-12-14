using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Agoda.Analyzers.Helpers;

namespace Agoda.Analyzers.AgodaCustom
{
    /// <summary>
    /// Base class from which we can permit or forbid the invocation of certain properties by defining their namespace,
    /// type and name. 
    /// </summary>
    public abstract class PropertyInvocationAnalyzerBase : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);
        protected abstract DiagnosticDescriptor Descriptor { get; }
        protected abstract IEnumerable<InvocationRule> Rules { get; }
        internal abstract Dictionary<string, string> Properties { get; }

        private static readonly Regex MatchGeneric = new Regex("<.*>$"); 

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeIdentifier, SyntaxKind.IdentifierName);
        }

        private void AnalyzeIdentifier(SyntaxNodeAnalysisContext context)
        {
            var identifierSyntax = (IdentifierNameSyntax) context.Node;
            var memberAccess = identifierSyntax?.Identifier.Parent?.Parent as MemberAccessExpressionSyntax;
            if (memberAccess == null) return;

            var memberType = context.SemanticModel.GetTypeInfo(memberAccess.Expression);
            if (!(memberType.Type is INamedTypeSymbol namedTypeSymbol))
            {
                return;
            }

            var constructedFrom = namedTypeSymbol.ConstructedFrom.ToDisplayString();
            // if it's a generic type then ignore the generic component
            constructedFrom = MatchGeneric.Replace(constructedFrom, "");

            if (Rules.Any(rule => !rule.Verify(constructedFrom, identifierSyntax.Identifier.Text)))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation()));
            }
        }
    }
}
