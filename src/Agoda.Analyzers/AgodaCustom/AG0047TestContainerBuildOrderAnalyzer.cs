using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Agoda.Analyzers.Helpers;
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

        private static readonly LocalizableString MessageFormat = "TestContainer Build() must be called after setting TESTCONTAINERS_HUB_IMAGE_NAME_PREFIX environment variable to ensure images are pulled from the correct mirror";

        private static readonly LocalizableString Description
            = DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0047TestContainerBuildOrderAnalyzer));

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Error,
            AnalyzerConstants.EnabledByDefault,
            Description,
            $"https://github.com/agoda-com/AgodaAnalyzers/blob/master/doc/{DiagnosticId}.md",
            WellKnownDiagnosticTags.EditAndContinue);

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
            var containingNode = invocation.Ancestors()
                .FirstOrDefault(node => 
                    node is MethodDeclarationSyntax || 
                    node is ConstructorDeclarationSyntax);
            
            if (containingNode == null)
            {
                return;
            }

            // Get the containing type
            var containingType = containingNode.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
            if (containingType == null)
            {
                return;
            }

            // Check if the environment variable is set in any method
            bool envVarSetInAnyMethod = false;
            var allMethods = containingType.Members.OfType<MethodDeclarationSyntax>();
            foreach (var method in allMethods)
            {
                if (IsEnvironmentVariableSetInNode(method, null))
                {
                    envVarSetInAnyMethod = true;
                    break;
                }
            }

            // If env var is set in any method, do not flag Build() calls in any other method
            if (envVarSetInAnyMethod && containingNode is MethodDeclarationSyntax)
            {
                return;
            }

            // Check if the environment variable is set before this Build() call
            if (!IsEnvironmentVariableSetBeforeBuild(containingType, containingNode, invocation))
            {
                var properties = ImmutableDictionary<string, string>.Empty.Add("VariableName", "TESTCONTAINERS_HUB_IMAGE_NAME_PREFIX");
                // Report diagnostic at the 'Build' identifier
                var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
                var buildIdentifier = memberAccess?.Name;
                var location = buildIdentifier != null ? buildIdentifier.GetLocation() : invocation.GetLocation();
                context.ReportDiagnostic(Diagnostic.Create(
                    Rule,
                    location,
                    properties));
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

            // Check if the type name contains "Container" or "Builder" and is from Testcontainers namespace
            var namespaceName = typeSymbol.ContainingNamespace?.ToString() ?? "";
            return (typeSymbol.Name.Contains("Container") || typeSymbol.Name.Contains("Builder")) &&
                   (namespaceName.Contains("Testcontainers") || namespaceName.Contains("DotNet.Testcontainers"));
        }

        private bool IsEnvironmentVariableSetBeforeBuild(
            TypeDeclarationSyntax containingType,
            SyntaxNode containingNode,
            InvocationExpressionSyntax buildCall)
        {
            // Check if the environment variable is set in the current method/constructor
            if (IsEnvironmentVariableSetInNode(containingNode, buildCall))
            {
                return true;
            }

            // Check if the environment variable is set in a setup method
            var setupMethods = containingType.Members
                .OfType<MethodDeclarationSyntax>()
                .Where(m => IsOneTimeSetUpMethod(m) || IsSetUpMethod(m));

            foreach (var setupMethod in setupMethods)
            {
                if (IsEnvironmentVariableSetInNode(setupMethod, null))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsEnvironmentVariableSetInNode(SyntaxNode node, InvocationExpressionSyntax buildCall)
        {
            var statements = GetStatements(node);
            if (statements == null)
            {
                return false;
            }

            if (buildCall != null)
            {
                var buildCallPosition = buildCall.GetLocation().SourceSpan.Start;
                var foundEnvVarSet = false;
                foreach (var statement in statements)
                {
                    if (statement.GetLocation().SourceSpan.Start >= buildCallPosition)
                    {
                        break;
                    }

                    if (IsEnvironmentVariableSetStatement(statement))
                    {
                        foundEnvVarSet = true;
                        break;  // Found the environment variable set before this Build() call
                    }
                }
                return foundEnvVarSet;
            }
            else
            {
                // If no build call is provided, check all statements
                return statements.Any(IsEnvironmentVariableSetStatement);
            }
        }

        private bool IsOneTimeSetUpMethod(MethodDeclarationSyntax method)
        {
            return method.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(a => a.Name.ToString() == "OneTimeSetUp" || a.Name.ToString() == "OneTimeSetUpAttribute");
        }

        private bool IsSetUpMethod(MethodDeclarationSyntax method)
        {
            return method.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(a => a.Name.ToString() == "SetUp" || a.Name.ToString() == "SetUpAttribute");
        }

        private IEnumerable<StatementSyntax> GetStatements(SyntaxNode node)
        {
            switch (node)
            {
                case MethodDeclarationSyntax method:
                    return method.Body?.Statements ?? 
                           method.ExpressionBody?.Expression?.Parent?.Parent?.ChildNodes().OfType<StatementSyntax>();
                
                case ConstructorDeclarationSyntax constructor:
                    return constructor.Body?.Statements ?? 
                           constructor.ExpressionBody?.Expression?.Parent?.Parent?.ChildNodes().OfType<StatementSyntax>();
                
                default:
                    return null;
            }
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