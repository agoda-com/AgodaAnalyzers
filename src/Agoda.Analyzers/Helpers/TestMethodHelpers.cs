using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Agoda.Analyzers.AgodaCustom;

namespace Agoda.Analyzers.Helpers
{
    public static class TestMethodHelpers
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

        public static readonly IEnumerable<PermittedInvocationRule> PermittedSeleniumSelectorRules = new[]
        {
            new WhitelistedInvocationRule("OpenQA.Selenium.By", "CssSelector"),
            new WhitelistedInvocationRule("OpenQA.Selenium.Remote.RemoteWebDriver",
                new Regex("^FindElement[s]?$"),
                new Regex("^FindElement[s]?ByCssSelector$"),
                new Regex("^ExecuteScript$"))
        };
    }
}
