using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom;

[TestFixture]
[Parallelizable(ParallelScope.All)]
class AG0032UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0032PreventUseOfBlockingTaskMethods();
        
    protected override string DiagnosticId => AG0032PreventUseOfBlockingTaskMethods.DIAGNOSTIC_ID;
        
    [Test]
    public async Task AG0032_WithTaskGetAwaiter_ShowsWarning()
    {
        var code = @"
				using System.Threading.Tasks;

				namespace Test 
				{
					class TestClass
					{
						public void TestMethod() 
						{
							var awaiter = Task.CompletedTask.GetAwaiter();
						}
					}
				}";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(10, 41));
    }
	    
    [Test]
    public async Task AG0032_WithTaskWait_ShowsWarning()
    {
        var code = @"
				using System.Threading.Tasks;

				namespace Test 
				{
					class TestClass 
					{
						public void TestMethod() 
						{
							var task = Task.CompletedTask;
							task.Wait();
						}
					}
				}";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(11, 13));
    }
	    
    [Test]
    public async Task AG0032_WithTaskWaitAll_ShowsWarning()
    {
        var code = @"
				using System.Threading.Tasks;

				namespace Test 
				{
					class TestClass 
					{
						public void TestMethod() 
						{
							var tasks = new [] {Task.CompletedTask};
							Task.WaitAll(tasks);
						}
					}
				}";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(11, 13));
    }
}