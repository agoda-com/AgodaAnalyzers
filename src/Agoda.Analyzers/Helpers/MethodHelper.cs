using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;

namespace Agoda.Analyzers.Helpers
{
    public static class MethodHelper
    {
        public static bool IsTestCase(MethodDeclarationSyntax methodDeclaration, SyntaxNodeAnalysisContext context)
        {
            // ensure public method
            if (!methodDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword)
                || methodDeclaration.IsKind(SyntaxKind.InterfaceDeclaration)
                || methodDeclaration.IsKind(SyntaxKind.ExplicitInterfaceSpecifier))
            {
                return false;
            }

            return context.SemanticModel
                .GetDeclaredSymbol(methodDeclaration)
                .GetAttributes()
                .Select(a => a.AttributeClass.BaseType.ToDisplayString())
                .Any(displayString => displayString == "NUnit.Framework.NUnitAttribute");
        }
    }
}
