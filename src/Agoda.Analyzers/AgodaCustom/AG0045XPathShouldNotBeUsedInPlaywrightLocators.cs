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
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0045XPathShouldNotBeUsedInPlaywrightLocators : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0045";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0045Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0045MessageFormat),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0045Description),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DIAGNOSTIC_ID,
            Title,
            MessageFormat,
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Error,
            AnalyzerConstants.EnabledByDefault,
            Description,
            $"https://github.com/agoda-com/AgodaAnalyzers/blob/master/doc/{DIAGNOSTIC_ID}.md",
            WellKnownDiagnosticTags.EditAndContinue);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        private static readonly Regex XPathPattern = new Regex(@"^(xpath=|//[a-zA-Z@\[\*]|\.\./|ancestor::|following-sibling::|preceding-sibling::|parent::|child::|descendant::|ancestor-or-self::|descendant-or-self::|following::|preceding::|self::)");

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var invocationExpression = (InvocationExpressionSyntax)context.Node;

            if (!(invocationExpression.Expression is MemberAccessExpressionSyntax memberAccess))
                return;

            var semanticModel = context.SemanticModel;
            var methodSymbol = semanticModel.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;
            
            if (methodSymbol == null || !IsPlaywrightLocatorMethod(methodSymbol))
                return;

            var firstArgument = invocationExpression.ArgumentList.Arguments.FirstOrDefault();
            if (firstArgument == null)
                return;

            // Check string literals
            if (firstArgument.Expression is LiteralExpressionSyntax literalExpression)
            {
                var value = literalExpression.Token.ValueText;
                if (XPathPattern.IsMatch(value))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, firstArgument.GetLocation(), properties: _props.ToImmutableDictionary()));
                }
                return;
            }

            // Check string variables
            if (firstArgument.Expression is IdentifierNameSyntax identifierName)
            {
                var symbol = semanticModel.GetSymbolInfo(identifierName).Symbol;
                if (symbol != null)
                {
                    string stringValue = null;

                    // Check local variables
                    if (symbol is ILocalSymbol localSymbol && localSymbol.Type.SpecialType == SpecialType.System_String)
                    {
                        // Try to get the initializer value
                        if (localSymbol.DeclaringSyntaxReferences.Length > 0)
                        {
                            var declaration = localSymbol.DeclaringSyntaxReferences[0].GetSyntax() as VariableDeclaratorSyntax;
                            if (declaration?.Initializer?.Value is LiteralExpressionSyntax initializer)
                            {
                                stringValue = initializer.Token.ValueText;
                            }
                        }
                    }
                    // Check fields
                    else if (symbol is IFieldSymbol fieldSymbol && fieldSymbol.Type.SpecialType == SpecialType.System_String)
                    {
                        // Try to get the initializer value
                        if (fieldSymbol.DeclaringSyntaxReferences.Length > 0)
                        {
                            var declaration = fieldSymbol.DeclaringSyntaxReferences[0].GetSyntax() as VariableDeclaratorSyntax;
                            if (declaration?.Initializer?.Value is LiteralExpressionSyntax initializer)
                            {
                                stringValue = initializer.Token.ValueText;
                            }
                        }
                    }
                    // Check properties
                    else if (symbol is IPropertySymbol propertySymbol && propertySymbol.Type.SpecialType == SpecialType.System_String)
                    {
                        // Try to get the initializer value
                        if (propertySymbol.DeclaringSyntaxReferences.Length > 0)
                        {
                            var declaration = propertySymbol.DeclaringSyntaxReferences[0].GetSyntax() as PropertyDeclarationSyntax;
                            if (declaration?.Initializer?.Value is LiteralExpressionSyntax initializer)
                            {
                                stringValue = initializer.Token.ValueText;
                            }
                        }
                    }

                    if (stringValue != null && XPathPattern.IsMatch(stringValue))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, firstArgument.GetLocation(), properties: _props.ToImmutableDictionary()));
                    }
                }
            }
        }

        private static bool IsPlaywrightLocatorMethod(IMethodSymbol methodSymbol)
        {
            var containingType = methodSymbol.ContainingType.ToString();
            return containingType == "Microsoft.Playwright.IPage" || 
                   containingType == "Microsoft.Playwright.Page" ||
                   containingType == "Microsoft.Playwright.ILocator" ||
                   containingType == "Microsoft.Playwright.Locator";
        }

        private static Dictionary<string, string> _props = new Dictionary<string, string>()
        {
            { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "10" }
        };
    }
} 