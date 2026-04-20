using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Agoda.Analyzers.Helpers;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0052PreventHardcodedTaskDelayInTests : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0052";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0052Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0052MessageFormat),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0052Description),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DIAGNOSTIC_ID,
            Title,
            MessageFormat,
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Warning,
            AnalyzerConstants.EnabledByDefault,
            Description,
            $"https://github.com/agoda-com/AgodaAnalyzers/blob/master/doc/{DIAGNOSTIC_ID}.md",
            WellKnownDiagnosticTags.EditAndContinue);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        private static readonly HashSet<string> TimeSpanFactoryMethods = new HashSet<string>
        {
            "FromMilliseconds",
            "FromSeconds",
            "FromMinutes",
            "FromHours",
            "FromDays",
            "FromTicks"
        };

        private static readonly HashSet<string> TestClassAttributes = new HashSet<string>
        {
            "NUnit.Framework.TestFixtureAttribute",
            "Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute",
        };

        private static readonly HashSet<string> TestMethodAttributes = new HashSet<string>
        {
            "NUnit.Framework.TestAttribute",
            "Xunit.FactAttribute",
            "Xunit.TheoryAttribute",
            "Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute",
        };

        private static readonly Dictionary<string, string> _props = new Dictionary<string, string>
        {
            { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "15" }
        };

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocationExpression, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if (!IsDelayOrSleepInvocation(invocation, context))
                return;

            if (!HasHardcodedDuration(invocation, context))
                return;

            if (IsInsideWhenAny(invocation, context))
                return;

            if (!IsInTestClass(invocation, context))
                return;

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptor,
                invocation.GetLocation(),
                properties: _props.ToImmutableDictionary()));
        }

        private static bool IsDelayOrSleepInvocation(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context)
        {
            var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
            if (!(symbolInfo.Symbol is IMethodSymbol methodSymbol))
                return false;

            var containingType = methodSymbol.ContainingType?.ToString();
            return (containingType == "System.Threading.Tasks.Task" && methodSymbol.Name == "Delay") ||
                   (containingType == "System.Threading.Thread" && methodSymbol.Name == "Sleep");
        }

        private static bool HasHardcodedDuration(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context)
        {
            if (invocation.ArgumentList.Arguments.Count == 0)
                return false;

            var firstArg = invocation.ArgumentList.Arguments[0].Expression;

            if (IsConstantNumericExpression(firstArg))
                return true;

            if (firstArg is InvocationExpressionSyntax innerInvocation)
            {
                var innerSymbol = context.SemanticModel.GetSymbolInfo(innerInvocation);
                if (innerSymbol.Symbol is IMethodSymbol innerMethod &&
                    innerMethod.ContainingType?.ToString() == "System.TimeSpan" &&
                    TimeSpanFactoryMethods.Contains(innerMethod.Name))
                {
                    if (innerInvocation.ArgumentList.Arguments.Count > 0)
                    {
                        return IsConstantNumericExpression(innerInvocation.ArgumentList.Arguments[0].Expression);
                    }
                }
            }

            if (firstArg is ObjectCreationExpressionSyntax objectCreation)
            {
                var typeInfo = context.SemanticModel.GetTypeInfo(objectCreation);
                if (typeInfo.Type?.ToString() == "System.TimeSpan" &&
                    objectCreation.ArgumentList?.Arguments.Count > 0)
                {
                    return objectCreation.ArgumentList.Arguments.All(
                        a => IsConstantNumericExpression(a.Expression));
                }
            }

            return false;
        }

        private static bool IsConstantNumericExpression(ExpressionSyntax expression)
        {
            if (expression is LiteralExpressionSyntax literal &&
                literal.IsKind(SyntaxKind.NumericLiteralExpression))
                return true;

            if (expression is BinaryExpressionSyntax binary)
                return IsConstantNumericExpression(binary.Left) && IsConstantNumericExpression(binary.Right);

            if (expression is ParenthesizedExpressionSyntax paren)
                return IsConstantNumericExpression(paren.Expression);

            return false;
        }

        private static bool IsInsideWhenAny(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context)
        {
            var current = invocation.Parent;
            while (current != null)
            {
                if (current is ArgumentSyntax &&
                    current.Parent is ArgumentListSyntax argList &&
                    argList.Parent is InvocationExpressionSyntax parentInvocation)
                {
                    var parentSymbol = context.SemanticModel.GetSymbolInfo(parentInvocation);
                    if (parentSymbol.Symbol is IMethodSymbol parentMethod &&
                        parentMethod.ContainingType?.ToString() == "System.Threading.Tasks.Task" &&
                        parentMethod.Name == "WhenAny")
                    {
                        return true;
                    }
                }

                if (current is StatementSyntax)
                    break;

                current = current.Parent;
            }

            return false;
        }

        private static bool IsInTestClass(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context)
        {
            var methodDecl = invocation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (methodDecl != null)
            {
                var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDecl);
                if (methodSymbol != null && methodSymbol.GetAttributes().Any(attr =>
                    TestMethodAttributes.Contains(attr.AttributeClass?.ToString())))
                {
                    return true;
                }
            }

            var classDecl = invocation.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (classDecl == null)
                return false;

            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDecl);
            if (classSymbol == null)
                return false;

            var current = classSymbol;
            while (current != null)
            {
                if (current.GetAttributes().Any(attr =>
                    TestClassAttributes.Contains(attr.AttributeClass?.ToString())))
                {
                    return true;
                }
                current = current.BaseType;
            }

            return false;
        }
    }
}
