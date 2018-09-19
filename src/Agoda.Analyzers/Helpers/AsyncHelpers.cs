using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace Agoda.Analyzers.Helpers
{
    public static class AsyncHelpers
    {
        private static readonly Regex MatchTaskReturnType = new Regex(@"^System\.Threading\.Tasks\.Task(<.*?[^>]>)?$");
        
        public static bool IsAsyncIntent(IMethodSymbol method)
        {
            return method.IsAsync || MatchTaskReturnType.IsMatch(method.ReturnType.ToDisplayString());
        }
    }
}