using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0044ForceOptionShouldNotBeUsed : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0044";
        private const string Category = "Usage";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0044Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0044MessageFormat),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0044Description),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DIAGNOSTIC_ID,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description,
            "https://github.com/agoda-com/AgodaAnalyzers/blob/master/doc/AG0044.md",
            WellKnownDiagnosticTags.EditAndContinue);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ObjectInitializerExpression);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var initializerExpression = (InitializerExpressionSyntax)context.Node;
            if (initializerExpression.Parent == null)
                return;

            // Check if this is initializing an options object for ILocator methods
            var typeInfo = context.SemanticModel.GetTypeInfo(initializerExpression.Parent);
            if (typeInfo.Type == null) return;

            var typeName = typeInfo.Type.Name;
            if (!IsLocatorOptionsType(typeName)) return;

            // Look for Force = true in the initializer
            foreach (var expression in initializerExpression.Expressions)
            {
                if (!(expression is AssignmentExpressionSyntax assignment) ||
                    !(assignment.Left is IdentifierNameSyntax identifier) ||
                    identifier.Identifier.ValueText != "Force") continue;
                
                if (!(assignment.Right is LiteralExpressionSyntax literal) ||
                    literal.Token.ValueText != "true") continue;
                
                var diagnostic = Diagnostic.Create(Rule, expression.GetLocation(), properties: _props.ToImmutableDictionary());
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsLocatorOptionsType(string typeName)
        {
            return typeName.EndsWith("Options") && (
                typeName.Contains("LocatorClick") ||
                typeName.Contains("LocatorFill") ||
                typeName.Contains("LocatorHover") ||
                typeName.Contains("LocatorDblClick") ||
                typeName.Contains("LocatorTap") ||
                typeName.Contains("LocatorCheck") ||
                typeName.Contains("LocatorUncheck") ||
                typeName.Contains("LocatorSelectOption"));
        }

        private static Dictionary<string, string> _props = new Dictionary<string, string>()
        {
            { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "60" }
        };
    }
}