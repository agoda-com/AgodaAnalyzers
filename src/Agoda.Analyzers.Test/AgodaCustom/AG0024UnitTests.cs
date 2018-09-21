using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    internal class AG0024UnitTests : DiagnosticVerifier
    {
        [Test]
        public async Task AG0024_Only_Method_Parameter_Should_Show_Warning()
        {
            var code = @"
using System.Threading;
using System.Threading.Tasks;

class TestClass 
{
    public void TestMethod1() 
    {
        Task.Factory.StartNew(MyMethod);
    }

    public void MyMethod()
    {
    }
}
";

            var baseResult = CSharpDiagnostic(AG0024PreventUseOfTaskFactoryStartNew.DIAGNOSTIC_ID);
            var expected = new[]
            {
                baseResult.WithLocation(9, 9)
            };

            await TestForResults(code, expected);
        }

        [Test]
        public async Task AG0024_TaskCreationOptions_DenyChildAttach_Parameter_Should_Show_Warning_For()
        {
            var code = @"
using System.Threading;
using System.Threading.Tasks;

class TestClass 
{
    public void TestMethod1() 
    {
        Task.Factory.StartNew(MyMethod, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
    }

    public void MyMethod()
    {
    }
}
";
            var baseResult = CSharpDiagnostic(AG0024PreventUseOfTaskFactoryStartNew.DIAGNOSTIC_ID);
            var expected = new[]
            {
                baseResult.WithLocation(9, 9)
            };

            await TestForResults(code, expected);
        }

        [Test]
        public async Task AG0024_TaskCreationOptions_LongRunning_Parameter_Should_Be_Allowed()
        {
            var code = @"
using System.Threading;
using System.Threading.Tasks;

class TestClass 
{
    public void TestMethod1() 
    {
        Task.Factory.StartNew(MyMethod, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    public void MyMethod()
    {
    }
}
";
            await TestForResults(code);
        }

        private async Task TestForResults(string code, DiagnosticResult[] expected = null)
        {
            expected = expected ?? new DiagnosticResult[0];
            var doc = CreateProject(new[] {code})
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] {doc}, CancellationToken.None)
                .ConfigureAwait(false);


            VerifyDiagnosticResults(diag, analyzersArray, expected);
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0024PreventUseOfTaskFactoryStartNew();
        }
    }
}