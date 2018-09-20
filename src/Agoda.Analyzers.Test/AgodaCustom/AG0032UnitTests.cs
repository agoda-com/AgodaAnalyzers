using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    class AG0032UnitTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0032PreventUseOfTaskWait();
        
        protected override string DiagnosticId => AG0032PreventUseOfTaskWait.DIAGNOSTIC_ID;
        
	    [Test]
        public async Task AG0032_WithTaskWait_ShowsWarning()
        {
	        var code = @"
				using System.Threading.Tasks;

				namespace Test 
				{
					class TestClass {
						public void TestMethod() 
						{
							var task = Task.CompletedTask;
							task.Wait();
						}
					}
				}";

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(10, 13));
        }
	    [Test]
	    public async Task AG0032_WithTaskWaitAll_ShowsWarning()
	    {
		    var code = @"
				using System.Threading.Tasks;

				namespace Test 
				{
					class TestClass {
						public void TestMethod() 
						{
							var tasks = new [] {Task.CompletedTask};
							Task.WaitAll(tasks);
						}
					}
				}";

		    await VerifyDiagnosticsAsync(code, new DiagnosticLocation(10, 13));
	    }
    }
}