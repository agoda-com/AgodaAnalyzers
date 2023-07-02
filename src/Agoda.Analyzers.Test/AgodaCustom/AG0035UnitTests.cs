using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
#if NET462
using System.Web;
#else
using Microsoft.AspNetCore.Http;
#endif
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom;

[TestFixture]
class AG0035UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0035PreventUseOfMachineName();
        
    protected override string DiagnosticId => AG0035PreventUseOfMachineName.DIAGNOSTIC_ID;

    [Test]
    public async Task AG0035_WithMachineNameFromEnvironment_ShowsWarning()
    {
        var code = @"				
				class TestClass
				{
					public void TestMethod() 
					{
						var machineName = System.Environment.MachineName;
					}
				}";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(6, 44));
    }
	    
}