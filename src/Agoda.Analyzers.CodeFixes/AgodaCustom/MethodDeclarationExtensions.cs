using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Agoda.Analyzers.CodeFixes.AgodaCustom
{
    public static class MethodDeclarationExtensions
    {
        public static MethodDeclarationSyntax WithAsyncKeyword(this MethodDeclarationSyntax methodDeclaration)
        {
            var asyncToken = SyntaxFactory.Token(SyntaxKind.AsyncKeyword)
                .WithTriviaFrom(methodDeclaration.Modifiers.FirstOrDefault());
            var modifiers = methodDeclaration.Modifiers.Replace(methodDeclaration.Modifiers.FirstOrDefault(), asyncToken);
            return methodDeclaration.WithModifiers(modifiers);
        }
    }
}