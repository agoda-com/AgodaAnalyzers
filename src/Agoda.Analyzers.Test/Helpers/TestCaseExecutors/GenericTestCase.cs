using Agoda.Analyzers.Test.Helpers.GenericTestHelpers;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Agoda.Analyzers.Test.Helpers.TestCaseExecutors
{
    public abstract class GenericTestCase : DiagnosticVerifier
    {
        public string Code { get; }
        public TestCaseProperties TestCaseProperties { get; }
        public IEnumerable<Assembly> ReferencedAssembies { get; }
        public DiagnosticAnalyzer DiagnosticAnalyzer { get; }

        public GenericTestCase(string code, TestCaseProperties testCaseProperties,
            IEnumerable<Assembly> referencedAssembies, DiagnosticAnalyzer diagnosticAnalyzer)
        {
            Code = code;
            TestCaseProperties = testCaseProperties;
            ReferencedAssembies = referencedAssembies;
            DiagnosticAnalyzer = diagnosticAnalyzer;
        }

        public abstract Task Execute();
    }
}
