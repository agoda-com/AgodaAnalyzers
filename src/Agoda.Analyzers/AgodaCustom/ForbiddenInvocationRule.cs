using System.Text.RegularExpressions;

namespace Agoda.Analyzers.AgodaCustom
{
    public class ForbiddenInvocationRule
    {
        public string NamespaceAndType { get; }
        public Regex ForbiddenIdentifierNameRegex { get; }

        private ForbiddenInvocationRule(string namespaceAndType, Regex forbiddenIdentifierNameRegex)
        {
            NamespaceAndType = namespaceAndType;
            ForbiddenIdentifierNameRegex = forbiddenIdentifierNameRegex;
        }

        public static ForbiddenInvocationRule Create(string namespaceAndType, Regex forbiddenMethodNameRegex)
            => new ForbiddenInvocationRule(namespaceAndType, forbiddenMethodNameRegex);
    }
}