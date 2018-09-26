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
        public TestCaseProperties TestCaseProperties;

        public GenericTestCase(TestCaseProperties testCaseProperties)
        {
            TestCaseProperties = testCaseProperties;
        }

        public abstract Task Execute();
    }
}
