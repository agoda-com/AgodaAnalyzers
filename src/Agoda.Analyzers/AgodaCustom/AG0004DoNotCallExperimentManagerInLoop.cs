using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0004DoNotCallExperimentManagerInLoop : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AG0002";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(CustomRulesResources.AG0002Title), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(CustomRulesResources.AG0002MessageFormat), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(CustomRulesResources.AG0002Description), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));
        private const string Category = "Usage";
        private static readonly string methodName = "IsB";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category, DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description
            );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ForStatement,SyntaxKind.ForEachStatement);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var foreachSyntax = (ForEachStatementSyntax)context.Node;

            var invocations =
              foreachSyntax.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault();

            var nodes = from node in foreachSyntax.DescendantNodes()
                                 .OfType<InvocationExpressionSyntax>()
                        let id = node.Expression as IdentifierNameSyntax
                        where id != null
                        where id.Identifier.GetType() == typeof()
                        select node;

            context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
        }
    }

    interface Test {
    }
}
