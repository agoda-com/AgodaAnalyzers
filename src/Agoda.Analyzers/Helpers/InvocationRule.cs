using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Agoda.Analyzers.Helpers
{
    /// <summary>
    /// Base class to represent permitted/forbidden method/property invocations.
    /// </summary>
    public abstract class InvocationRule
    {
        private readonly string _namespaceAndType;
        private readonly bool _isBlacklist;
        private readonly Regex[] _names;

        protected InvocationRule(string namespaceAndType, bool isBlacklist, string[] names)
            : this(namespaceAndType, isBlacklist, names.Select(name => new Regex($"^{name}$")).ToArray())
        {}
            
        protected InvocationRule(string namespaceAndType, bool isBlacklist, params Regex[] names)
        {
            _namespaceAndType = namespaceAndType;
            _isBlacklist = isBlacklist;
            _names = names;
        }

        /// <summary>
        /// Verifies the given context's Node complies with the rule.
        /// </summary>
        public bool Verify(SyntaxNodeAnalysisContext context)
        {
            var invocationExpressionSyntax = (InvocationExpressionSyntax) context.Node;
            if (!(context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol is IMethodSymbol methodSymbol))
            {
                return true;
            }
            
            return Verify(methodSymbol);
        }
        
        /// <summary>
        /// Verifies the given method symbol complies with the rule.
        /// </summary>
        public bool Verify(IMethodSymbol methodSymbol)
        {
            return Verify(methodSymbol.ContainingType.ConstructedFrom.ToDisplayString(), methodSymbol.Name);
        }
        
        /// <summary>
        /// Verifies the namespace, type name and method name comply with the rule.
        /// </summary>
        public bool Verify(string namespaceAndType, string name)
        {
            if (namespaceAndType != _namespaceAndType)
            {
                return true;
            }
            var isPermitted = _names.Any(regex => regex.IsMatch(name));
            return _isBlacklist ? !isPermitted : isPermitted;
        }
        
        /// <summary>
        /// Determines if the context's Node matches the namespace, type and name of the rule. 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool IsMatch(SyntaxNodeAnalysisContext context)
        {
            var invocationExpressionSyntax = (InvocationExpressionSyntax) context.Node;
            if (!(context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol is IMethodSymbol methodSymbol))
            {
                return false;
            }
            
            return IsMatch(methodSymbol.ContainingType.ConstructedFrom.ToDisplayString(), methodSymbol.Name);
        }

        private bool IsMatch(string namespaceAndType, string name)
        {
            return namespaceAndType == _namespaceAndType && _names.Any(regex => regex.IsMatch(name));
        }
    }

    internal class WhitelistedInvocationRule : InvocationRule
    {
        public WhitelistedInvocationRule(string namespaceAndType, params string[] names)
            : base(namespaceAndType, false, names)
        {}
        
        public WhitelistedInvocationRule(string namespaceAndType, params Regex[] names)
            : base(namespaceAndType, false, names)
        {}
    }
    
    internal class BlacklistedInvocationRule : InvocationRule
    {
        public BlacklistedInvocationRule(string namespaceAndType, params string[] names)
            : base(namespaceAndType, true, names)
        {}
        
        public BlacklistedInvocationRule(string namespaceAndType, params Regex[] names)
            : base(namespaceAndType, true, names)
        {
        }
    }
}
