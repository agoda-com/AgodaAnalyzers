using System.Collections.Generic;
using System.Collections.Immutable;
using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0042ElementHandlerShouldNotBeUsed : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0042";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0042Title), CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0042Title), CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description
            = DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0042ElementHandlerShouldNotBeUsed));

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DIAGNOSTIC_ID,
            Title,
            MessageFormat,
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Warning,
            AnalyzerConstants.EnabledByDefault,
            Description,
            $"https://github.com/agoda-com/AgodaAnalyzers/blob/master/doc/{DIAGNOSTIC_ID}.md",
            WellKnownDiagnosticTags.EditAndContinue);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var invocationExpression = (InvocationExpressionSyntax)context.Node;

            if (!(invocationExpression.Expression is MemberAccessExpressionSyntax memberAccess))
                return;

            var semanticModel = context.SemanticModel;
            
            // Check the caller type (should be IPage or IElementHandle)
            var callerSymbolInfo = semanticModel.GetSymbolInfo(memberAccess.Expression);
            var callerSymbol = callerSymbolInfo.Symbol;
            if (callerSymbol == null) return;

            var callerTypeSymbol = GetTypeSymbol(callerSymbol);
            if (callerTypeSymbol == null) return;

            if (!IsPlaywrightPageOrElementType(callerTypeSymbol)) return;

            // Check the method return type
            var methodSymbol = (IMethodSymbol)semanticModel.GetSymbolInfo(invocationExpression).Symbol;
            if (methodSymbol == null) return;

            // Get the return type
            var returnType = methodSymbol.ReturnType;
            if (!IsElementHandleReturnType(returnType)) return;

            var diagnostic = Diagnostic.Create(Descriptor, invocationExpression.GetLocation(), properties: _props.ToImmutableDictionary());
            context.ReportDiagnostic(diagnostic);
        }

        private static bool IsElementHandleReturnType(ITypeSymbol returnType)
        {
            // Handle Task<IElementHandle> and Task<IElementHandle?>
            if (!(returnType is INamedTypeSymbol namedType) || !namedType.IsGenericType) 
                return false;
            
            var taskType = namedType.ConstructedFrom;
            if (taskType.ToString() != "System.Threading.Tasks.Task<TResult>" && taskType.ToString() != "System.Threading.Tasks.Task<T>") 
                return false;
            
            var typeArg = namedType.TypeArguments[0];
                    
            // Handle nullable type
            if (typeArg is INamedTypeSymbol nullableType && nullableType.IsGenericType)
            {
                typeArg = nullableType.TypeArguments[0];
            }

            return IsElementHandleType(typeArg);

        }

        private static bool IsElementHandleType(ITypeSymbol typeSymbol)
        {
            var typeName = typeSymbol.ToString();
            return typeName == "Microsoft.Playwright.IElementHandle" ||
                   typeName == "Microsoft.Playwright.ElementHandle" ||
                   typeName == "Microsoft.Playwright.IElementHandle?" ||
                   typeName == "Microsoft.Playwright.ElementHandle?";
        }

        private static INamedTypeSymbol GetTypeSymbol(ISymbol symbol)
        {
            switch (symbol)
            {
                case ILocalSymbol localSymbol:
                    return localSymbol.Type as INamedTypeSymbol;
                case IParameterSymbol parameterSymbol:
                    return parameterSymbol.Type as INamedTypeSymbol;
                case IFieldSymbol fieldSymbol:
                    return fieldSymbol.Type as INamedTypeSymbol;
                case IPropertySymbol propertySymbol:
                    return propertySymbol.Type as INamedTypeSymbol;
                default:
                    return null;
            }
        }

        private static bool IsPlaywrightPageOrElementType(INamedTypeSymbol typeSymbol)
        {
            var typeName = typeSymbol.ToString();
            return typeName == "Microsoft.Playwright.IPage" ||
                   typeName == "Microsoft.Playwright.Page" ||
                   typeName == "Microsoft.Playwright.IElementHandle" ||
                   typeName == "Microsoft.Playwright.ElementHandle";
        }

        private static Dictionary<string, string> _props = new Dictionary<string, string>()
        {
            { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "10" }
        };
    }
}