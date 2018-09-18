using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Agoda.Analyzers.Helpers
{
    public abstract class PermittedInvocationRule
    {
        private readonly string _namespaceAndType;
        private readonly bool _isBlacklist;
        private readonly Regex[] _names;

        protected PermittedInvocationRule(string namespaceAndType, bool isBlacklist, string[] names)
            : this(namespaceAndType, isBlacklist, names.Select(name => new Regex($"^{name}$")).ToArray())
        {}
            
        protected PermittedInvocationRule(string namespaceAndType, bool isBlacklist, params Regex[] names)
        {
            _namespaceAndType = namespaceAndType;
            _isBlacklist = isBlacklist;
            _names = names;
        }

        public bool Verify(SyntaxNodeAnalysisContext context)
        {
            var invocationExpressionSyntax = (InvocationExpressionSyntax) context.Node;
            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol as IMethodSymbol;
            return Verify(methodSymbol.ContainingType.ConstructedFrom.ToDisplayString(), methodSymbol.Name);
        }
                
        public bool Verify(string namespaceAndType, string name)
        {
            if (namespaceAndType != _namespaceAndType)
            {
                return true;
            }
            var isPermitted = _names.Any(regex => regex.IsMatch(name));
            return _isBlacklist ? !isPermitted : isPermitted;
        }
    }

    internal class WhitelistedInvocationRule : PermittedInvocationRule
    {
        public WhitelistedInvocationRule(string namespaceAndType, params string[] names)
            : base(namespaceAndType, false, names)
        {}
        
        public WhitelistedInvocationRule(string namespaceAndType, params Regex[] names)
            : base(namespaceAndType, false, names)
        {}
    }
    
    internal class BlacklistedInvocationRule : PermittedInvocationRule
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
