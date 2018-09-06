// <copyright file="AG0018EnsureThatPubliclyExposedIEnumerableTypes.cs" company="Agoda Company Co., Ltd.">
// AGODA ® is a registered trademark of AGIP LLC, used under license by Agoda Company Co., Ltd.. Agoda is part of Priceline (NASDAQ:PCLN)
// </copyright>
using System;
using System.Collections;
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
    public class AG0018EnsureThatPubliclyExposedIEnumerableTypes : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AG0018";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(CustomRulesResources.AG0018Title), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(CustomRulesResources.AG0018Title), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));
        private static readonly LocalizableString Description = DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0018EnsureThatPubliclyExposedIEnumerableTypes));

        private static readonly DiagnosticDescriptor Descriptor =
          new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, AnalyzerCategory.CustomQualityRules,
              DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, null, WellKnownDiagnosticTags.EditAndContinue);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeReturnType, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzePropertyType, SyntaxKind.PropertyDeclaration);
        }

        private void AnalyzeReturnType(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            if (IsPubliclyExposedIEnumerableTypes(context)) { return; }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, methodDeclaration.GetLocation()));
        }

        private void AnalyzePropertyType(SyntaxNodeAnalysisContext context)
        {
            var propertyDeclarationSyntax = (PropertyDeclarationSyntax)context.Node;

            if (IsPubliclyExposedIEnumerableTypes(context)) { return; }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, propertyDeclarationSyntax.GetLocation()));
        }

        private bool IsPubliclyExposedIEnumerableTypes(SyntaxNodeAnalysisContext context)
        {
            INamedTypeSymbol namedTypeSymbol = null;
            bool isArray = false;
            if (context.ContainingSymbol.Kind == SymbolKind.Method)
            {
                IMethodSymbol method = (IMethodSymbol)context.ContainingSymbol;
                isArray = method.ReturnType is IArrayTypeSymbol;
                namedTypeSymbol = method.ReturnType as INamedTypeSymbol;
                
            }
            else if (context.ContainingSymbol.Kind == SymbolKind.Property)
            {
                IPropertySymbol property = (IPropertySymbol)context.ContainingSymbol;
                isArray = property is IArrayTypeSymbol;
                namedTypeSymbol = property.Type as INamedTypeSymbol;
                
            }

            if (isArray ||
                    (namedTypeSymbol != null
                     && namedTypeSymbol.ContainingNamespace.ToDisplayString().StartsWith("System.Collections")
                     && namedTypeSymbol.ConstructedFrom.Interfaces.Any(x => x.ToDisplayString() == "System.Collections.IEnumerable")
                     && namedTypeSymbol.ConstructedFrom.ToDisplayString() != "string"))
            {
                var fullName = namedTypeSymbol.OriginalDefinition.ToDisplayString();
                if (fullName == "System.Collections.Generic.ISet<T>"
                    || fullName == "System.Collections.Generic.IList<T>"
                    || fullName == "System.Collections.Generic.IDictionary<TKey, TValue>"
                    || fullName == "System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>"
                    || fullName == "System.Collections.Generic.KeyedCollection<TKey, TValue>")
                {
                    return true;
                }

                return false;
            }

            return true;
        }
    }
}