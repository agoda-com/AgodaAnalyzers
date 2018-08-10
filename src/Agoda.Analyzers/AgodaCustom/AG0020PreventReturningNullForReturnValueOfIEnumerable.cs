﻿using System.Collections.Immutable;
using System.Linq;
using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0020PreventReturningNullForReturnValueOfIEnumerable : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AG0020";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0020Title), CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0020Title), CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description =
            DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0020PreventReturningNullForReturnValueOfIEnumerable));

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, AnalyzerCategory.CustomQualityRules,
                DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, null,
                WellKnownDiagnosticTags.EditAndContinue);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, new[] {
                SyntaxKind.ReturnStatement,
                SyntaxKind.ArrowExpressionClause,
            });
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        private bool ContainsNullExpression(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.IsKind(SyntaxKind.ReturnStatement))
            {
                ReturnStatementSyntax statement = (ReturnStatementSyntax)context.Node;
                return statement.Expression.IsKind(SyntaxKind.NullLiteralExpression);
            }
            else if (context.Node.IsKind(SyntaxKind.ArrowExpressionClause))
            {
                ArrowExpressionClauseSyntax statement = (ArrowExpressionClauseSyntax)context.Node;
                return statement.Expression.IsKind(SyntaxKind.NullLiteralExpression);
            }
            return false;
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (ContainsNullExpression(context))
            {
                if (context.ContainingSymbol.Kind == SymbolKind.Method)
                {
                    IMethodSymbol method = (IMethodSymbol)context.ContainingSymbol;
                    var methodReturnType = method.ReturnType as INamedTypeSymbol;
                    if (methodReturnType?.ConstructedFrom.Interfaces.Any(x => x.ToDisplayString() == "System.Collections.IEnumerable") == true
                        && methodReturnType.ConstructedFrom.ToDisplayString() != "string")
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation()));
                    }
                }
            }
        }
    }
}