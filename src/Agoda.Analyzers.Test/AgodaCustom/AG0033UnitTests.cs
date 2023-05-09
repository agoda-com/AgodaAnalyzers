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

namespace Agoda.Analyzers.Test.AgodaCustom;

[TestFixture]
class AG0033UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0033PreventUseOfTaskResult();
        
    protected override string DiagnosticId => AG0033PreventUseOfTaskResult.DIAGNOSTIC_ID;
        
    [Test]
    public async Task AG0032_WithTaskResult_ShowsWarning()
    {
        var code = @"
				using System.Threading.Tasks;

				namespace Test 
				{
					class TestClass
					{
						public void TestMethod() 
						{
							var task = Task.FromResult(0);
							var result = task.Result;
						}
					}
				}";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(11, 26));
    }
}