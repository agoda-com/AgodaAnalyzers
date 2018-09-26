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
    /// <summary>
    /// Abstract GenericTestCase from which different type of Generic Test Cases extend
    /// </summary>
    public abstract class GenericTestCase : DiagnosticVerifier
    {
        public TestCaseProperties TestCaseProperties { get; set; }

        public GenericTestCase(TestCaseProperties testCaseProperties)
        {
            TestCaseProperties = testCaseProperties;
        }

        /// <summary>
        /// Executing specific test case
        /// </summary>
        /// <returns>Task for async execution</returns>
        public abstract Task Execute();
    }
}
