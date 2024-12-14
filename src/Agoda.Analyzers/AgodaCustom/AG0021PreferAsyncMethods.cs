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
    public class AG0021PreferAsyncMethods : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "AG0021";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0021Title), 
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0021Title), 
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description =
            DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0021PreferAsyncMethods));

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DIAGNOSTIC_ID, 
            Title, 
            MessageFormat, 
            AnalyzerCategory.CustomQualityRules,
            DiagnosticSeverity.Info, 
            AnalyzerConstants.EnabledByDefault, 
            Description,
            $"https://github.com/agoda-com/AgodaAnalyzers/blob/master/doc/{DIAGNOSTIC_ID}.md", 
            WellKnownDiagnosticTags.EditAndContinue);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax) context.Node;

            if (!(GetMethodDescriptor(context.SemanticModel, invocation) is MethodDescriptor descriptor))
            {
                return;
            }

            if (descriptor.UsedMethod.IsAwaitableNonDynamic(context.SemanticModel, context.Node.SpanStart))
            {
                return;
            }

            IEnumerable<IMethodSymbol> alternativeAsyncMethods =
                GetAlternativeAsyncMethods(context.SemanticModel, context.Node.SpanStart, descriptor);

            if (alternativeAsyncMethods.Any())
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation(), properties: _props.ToImmutableDictionary()));
            }
        }

        private MethodDescriptor? GetMethodDescriptor(SemanticModel semanticModel, InvocationExpressionSyntax invocation)
        {
            MethodDescriptor? GetDescriptor()
            {
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    var usedMethod = (IMethodSymbol) semanticModel.GetSymbolInfo(memberAccess).Symbol;

                    // method called on non dynamic object
                    if (usedMethod != null)
                    {
                        var callingSymbol = semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;

                        // Method called on a result from another method
                        if (callingSymbol is IMethodSymbol methodSymbol)
                        {
                            return new MethodDescriptor
                            {
                                CallingType = methodSymbol.ReturnType,
                                UsedMethod = usedMethod,
                            };
                        }

                        // Method called on a local variable, static class, property, field, etc.
                        return new MethodDescriptor
                        {
                            CallingType = semanticModel.GetTypeInfo(memberAccess.Expression).Type,
                            UsedMethod = usedMethod,
                        };
                    }
                }

                if (invocation.Expression is IdentifierNameSyntax identifierName)
                {
                    var usedMethod = semanticModel.GetSymbolInfo(identifierName).Symbol;

                    // Method called without explicit context (a class own method is inside the class)
                    if (usedMethod is IMethodSymbol methodSymbol)
                    {
                        return new MethodDescriptor
                        {
                            CallingType = methodSymbol.ContainingType,
                            UsedMethod = methodSymbol,
                        };
                    }
                }

                return null;
            }

            if (!(GetDescriptor() is MethodDescriptor descriptor))
            {
                return null;
            }

            // if method is extension but used in static mode, replace calling type thr real one.
            if (descriptor.UsedMethod.IsExtensionMethod && descriptor.UsedMethod.ContainingType.Equals(descriptor.CallingType))
            {
                descriptor.CallingType = descriptor.UsedMethod.Parameters[0].Type;
            }

            return descriptor;
        }

        private IEnumerable<IMethodSymbol> GetAlternativeAsyncMethods(SemanticModel semanticModel, int position, MethodDescriptor descriptor)
        {
            // searching methods by name
            var methods = semanticModel.LookupSymbols(
                position,
                container: descriptor.CallingType,
                name: descriptor.UsedMethod.Name,
                includeReducedExtensionMethods: true).OfType<IMethodSymbol>();

            // searching methods by name with the postfix
            if (!descriptor.UsedMethod.Name.EndsWith("Async", StringComparison.Ordinal))
            {
                methods = methods.Concat(semanticModel.LookupSymbols(
                    position,
                    container: descriptor.CallingType,
                    name: descriptor.UsedMethod.Name + "Async",
                    includeReducedExtensionMethods: true).OfType<IMethodSymbol>());
            }

            // should we look at static method
            bool isStatic = descriptor.UsedMethod.IsStatic && !descriptor.UsedMethod.IsExtensionMethod;

            foreach (IMethodSymbol member in methods)
            {
                if (!member.Equals(descriptor.UsedMethod)
                    && (member.IsStatic == isStatic || member.IsExtensionMethod)
                    && member.IsAwaitableNonDynamic(semanticModel, position: 0))
                {
                    yield return member;
                }
            }
        }

        private struct MethodDescriptor
        {
            /// <summary>
            /// Symbol of used method
            /// </summary>
            public IMethodSymbol UsedMethod { get; set; }

            /// <summary>
            /// Symbol of a type which is used for calling the used method
            /// </summary>
            public ITypeSymbol CallingType { get; set; }
        }
        private static Dictionary<string, string> _props = new Dictionary<string, string>()
        {
            { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "10" }
        };
    }
}
