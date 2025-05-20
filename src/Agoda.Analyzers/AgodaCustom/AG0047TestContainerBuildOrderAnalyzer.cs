using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0047TestContainerBuildOrderAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AG0047";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0047Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0047MessageFormat),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0047Description),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            AnalyzerCategory.Custom,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMethodInvocation, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeMethodInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            
            // Check if this is a Build() method call
            if (!IsBuildMethodCall(invocation))
            {
                return;
            }

            // Check if this is a TestContainer Build() call
            if (!IsTestContainerBuildCall(invocation, context))
            {
                return;
            }

            // Get the containing method or constructor
            var containingMethod = invocation.Ancestors()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault();
            
            if (containingMethod == null)
            {
                return;
            }

            // Check if the environment variable is set before this Build() call
            if (!IsEnvironmentVariableSetBeforeBuild(containingMethod, invocation, context))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Rule,
                    invocation.GetLocation(),
                    "TESTCONTAINERS_HUB_IMAGE_NAME_PREFIX"));
            }
        }

        private bool IsBuildMethodCall(InvocationExpressionSyntax invocation)
        {
            return invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                   memberAccess.Name.Identifier.Text == "Build";
        }

        private bool IsTestContainerBuildCall(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context)
        {
            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccess == null)
            {
                return false;
            }

            var semanticModel = context.SemanticModel;
            var typeInfo = semanticModel.GetTypeInfo(memberAccess.Expression);

            // Check if the type is a TestContainer or derived from it
            var typeSymbol = typeInfo.Type;
            if (typeSymbol == null)
            {
                return false;
            }

            // Check if the type name contains "Container" and is from Testcontainers namespace
            return typeSymbol.Name.Contains("Container") &&
                   typeSymbol.ContainingNamespace?.ToString().Contains("Testcontainers") == true;
        }

        private bool IsEnvironmentVariableSetBeforeBuild(
            MethodDeclarationSyntax containingMethod,
            InvocationExpressionSyntax buildCall,
            SyntaxNodeAnalysisContext context)
        {
            var statements = containingMethod.Body?.Statements ?? containingMethod.ExpressionBody?.Expression?.Parent?.Parent?.ChildNodes().OfType<StatementSyntax>();
            if (statements == null)
            {
                return false;
            }

            var buildCallPosition = buildCall.GetLocation().SourceSpan.Start;
            
            foreach (var statement in statements)
            {
                if (statement.GetLocation().SourceSpan.Start >= buildCallPosition)
                {
                    break;
                }

                if (IsEnvironmentVariableSetStatement(statement))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsEnvironmentVariableSetStatement(SyntaxNode statement)
        {
            if (statement is ExpressionStatementSyntax expressionStatement)
            {
                if (expressionStatement.Expression is InvocationExpressionSyntax invocation)
                {
                    var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
                    if (memberAccess?.Name.Identifier.Text == "SetEnvironmentVariable")
                    {
                        var arguments = invocation.ArgumentList.Arguments;
                        if (arguments.Count >= 1)
                        {
                            var firstArg = arguments[0].Expression as LiteralExpressionSyntax;
                            return firstArg?.Token.ValueText == "TESTCONTAINERS_HUB_IMAGE_NAME_PREFIX";
                        }
                    }
                }
            }

            return false;
        }
    }
} 