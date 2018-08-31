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
        public const string DiagnosticId = "AG0021";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0021Title), CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0021Title), CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description =
            DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0021PreferAsyncMethods));

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, AnalyzerCategory.CustomQualityRules,
                DiagnosticSeverity.Info, AnalyzerConstants.EnabledByDefault, Description, null,
                WellKnownDiagnosticTags.EditAndContinue);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax) context.Node;
            
            MethodDescriptor? descriptor = GetMethodDescriptor(context.SemanticModel, invocation);

            if (!descriptor.HasValue || 
                descriptor.Value.UsedMethod.IsAwaitableNonDynamic(context.SemanticModel, invocation.Expression.SpanStart))
            {
                return;
            }

            MethodDescriptor d = descriptor.Value;

            if (d.UsedMethod.IsExtensionMethod && d.UsedMethod.ContainingType.Equals(d.CallingType))
            {
                d.CallingType = descriptor.Value.UsedMethod.Parameters[0].Type;
            }

            var asyncAlternativeMethods = GetAlternativeMethods(context.SemanticModel, d);

            if (asyncAlternativeMethods.Any())
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.GetLocation()));
            }
        }

        private MethodDescriptor? GetMethodDescriptor(SemanticModel semanticModel, InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                var callingSymbol = semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;
                var usedMethod = (IMethodSymbol) semanticModel.GetSymbolInfo(memberAccess).Symbol;
                
                if (callingSymbol is IMethodSymbol methodSymbol)
                {
                    return new MethodDescriptor
                    {
                        CallingType = methodSymbol.ReturnType,
                        UsedMethod = usedMethod,
                    };
                }

                return new MethodDescriptor
                {
                    CallingType = semanticModel.GetTypeInfo(memberAccess.Expression).Type,
                    UsedMethod = usedMethod,
                };
            }

            if (invocation.Expression is IdentifierNameSyntax identifierName)
            {
                var callingSymbol = semanticModel.GetSymbolInfo(identifierName).Symbol;

                if (callingSymbol is IMethodSymbol methodSymbol)
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

        internal struct MethodDescriptor
        {
            public IMethodSymbol UsedMethod { get; set; }

            public ITypeSymbol CallingType { get; set; }
        }

        private IEnumerable<ISymbol> GetAlternativeMethods(SemanticModel semanticModel, MethodDescriptor descriptor)
        {
            string postfixName = descriptor.UsedMethod.Name + "Async";

            var methods = descriptor.CallingType.GetMembersInThisAndBaseTypes<IMethodSymbol>()
                .Concat(descriptor.UsedMethod.ContainingType.GetMembersInThisAndBaseTypes<IMethodSymbol>());

            var isStatic = descriptor.UsedMethod.IsStatic && !descriptor.UsedMethod.IsExtensionMethod;

            foreach (var member in methods)
            {
                if (!member.Equals(descriptor.UsedMethod)
                    && (member.IsStatic == isStatic || member.IsExtensionMethod)
                    && member.IsAwaitableNonDynamic(semanticModel, 0)
                    && (member.Name == descriptor.UsedMethod.Name || member.Name == postfixName))
                {
                    yield return member;
                }
            }
        }
    }
}