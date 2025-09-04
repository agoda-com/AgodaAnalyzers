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

        private static readonly Regex XPathPattern = new Regex(@"^(xpath=|//|\.\./|/\*|ancestor::|following-sibling::|preceding-sibling::|parent::|child::|descendant::|ancestor-or-self::|descendant-or-self::|following::|preceding::|self::|\$)");

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocationExpression, SyntaxKind.InvocationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeVariableDeclaration, SyntaxKind.VariableDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzePropertyDeclaration, SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeReturnStatement, SyntaxKind.ReturnStatement);
            context.RegisterSyntaxNodeAction(AnalyzeInterpolatedString, SyntaxKind.InterpolatedStringExpression);
        }

        private static void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
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

            // Don't check string variables here to avoid duplicate reporting
            // Variable declarations are already checked in AnalyzeVariableDeclaration
        }

        private static void AnalyzeVariableDeclaration(SyntaxNodeAnalysisContext context)
        {
            var variableDeclaration = (VariableDeclarationSyntax)context.Node;
            
            // Check if it's a string type
            if (variableDeclaration.Type.ToString() != "string")
                return;

            foreach (var variable in variableDeclaration.Variables)
            {
                if (variable.Initializer?.Value is LiteralExpressionSyntax literalExpression)
                {
                    var value = literalExpression.Token.ValueText;
                    if (IsXPathRelatedDeclaration(variable.Identifier.ValueText, value))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, literalExpression.GetLocation(), properties: _props.ToImmutableDictionary()));
                    }
                }
            }
        }

        private static void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
        {
            var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;
            
            // Check if it's a string type
            if (propertyDeclaration.Type.ToString() != "string")
                return;

            if (propertyDeclaration.Initializer?.Value is LiteralExpressionSyntax literalExpression)
            {
                var value = literalExpression.Token.ValueText;
                if (IsXPathRelatedDeclaration(propertyDeclaration.Identifier.ValueText, value))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, literalExpression.GetLocation(), properties: _props.ToImmutableDictionary()));
                }
            }
        }

        private static void AnalyzeReturnStatement(SyntaxNodeAnalysisContext context)
        {
            var returnStatement = (ReturnStatementSyntax)context.Node;
            
            if (returnStatement.Expression is LiteralExpressionSyntax literalExpression)
            {
                var value = literalExpression.Token.ValueText;
                if (XPathPattern.IsMatch(value))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, literalExpression.GetLocation(), properties: _props.ToImmutableDictionary()));
                }
            }
            else if (returnStatement.Expression is InterpolatedStringExpressionSyntax interpolatedString)
            {
                AnalyzeInterpolatedStringContent(context, interpolatedString);
            }
        }

        private static void AnalyzeInterpolatedString(SyntaxNodeAnalysisContext context)
        {
            var interpolatedString = (InterpolatedStringExpressionSyntax)context.Node;
            
            // Skip if this interpolated string is part of a return statement
            // to avoid duplicate reporting (return statements are handled separately)
            if (interpolatedString.Parent is ReturnStatementSyntax)
                return;
                
            AnalyzeInterpolatedStringContent(context, interpolatedString);
        }

        private static void AnalyzeInterpolatedStringContent(SyntaxNodeAnalysisContext context, InterpolatedStringExpressionSyntax interpolatedString)
        {
            // Check if the interpolated string starts with XPath patterns
            var firstContent = interpolatedString.Contents.FirstOrDefault();
            if (firstContent is InterpolatedStringTextSyntax textContent)
            {
                var value = textContent.TextToken.ValueText;
                if (XPathPattern.IsMatch(value))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, interpolatedString.GetLocation(), properties: _props.ToImmutableDictionary()));
                }
            }
        }

        private static bool IsXPathRelatedDeclaration(string identifierName, string value)
        {
            // Check if the identifier name suggests it's XPath-related
            var isXPathNamed = identifierName.ToLowerInvariant().Contains("xpath");
            
            // Check if the value contains XPath patterns
            var containsXPath = XPathPattern.IsMatch(value);
            
            // Report if either the name suggests XPath or the value contains XPath patterns
            return isXPathNamed || containsXPath;
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