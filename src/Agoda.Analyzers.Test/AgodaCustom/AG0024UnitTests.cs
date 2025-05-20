using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom;

[TestFixture]
[Parallelizable(ParallelScope.All)]
internal class AG0024UnitTests : DiagnosticVerifier
{

    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0024PreventUseOfTaskFactoryStartNew();

    protected override string DiagnosticId => AG0024PreventUseOfTaskFactoryStartNew.DIAGNOSTIC_ID;

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

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(9, 9));
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
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(9, 9));
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
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }
}