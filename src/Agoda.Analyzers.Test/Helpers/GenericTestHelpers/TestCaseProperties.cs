using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agoda.Analyzers.Test.Helpers.GenericTestHelpers
{
    public class TestCaseProperties
    {
        public string DiagnosticId { get; }
        public bool IsWarning { get; }
        public string Name { get; }

        public TestCaseProperties(string testName, string testCasePrefix)
        {
            var withoutPrefix = testName.Replace(testCasePrefix, String.Empty);
            var tokens = withoutPrefix.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            DiagnosticId = tokens[0];
            string warningValue = tokens[1];
            IsWarning = warningValue.ToLower() == "warning";
            Name = withoutPrefix.Replace($".{DiagnosticId}.{warningValue}.", String.Empty);
        }
    }
}
