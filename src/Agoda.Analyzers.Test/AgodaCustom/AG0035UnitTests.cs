using System.Threading.Tasks;
using System.Web;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom
{
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
	    
	    [Test]
	    public async Task AG0035_WithMachineNameFromHttpContext_ShowsWarning()
	    {
		    var code = new CodeDescriptor
		    {
			    References = new[] {typeof(HttpContext).Assembly},
			    Code = @"
					class TestClass
					{
						public void TestMethod() 
						{
							var machineName = System.Web.HttpContext.Current.Server.MachineName;
						}
					}"
		    };

		    await VerifyDiagnosticsAsync(code, new DiagnosticLocation(6, 64));
	    }
	    
	    [Test]
	    public async Task AG0035_WithMachineNameFromHttpContextWrapper_ShowsWarning()
	    {
		    var code = new CodeDescriptor
		    {
			    References = new[] {typeof(HttpContext).Assembly},
			    Code = @"
					using System.Web;

					class TestClass
					{
						public void TestMethod() 
						{
							var wrapper = new HttpContextWrapper(HttpContext.Current);
							var machineName = wrapper.Server.MachineName;
						}
					}"
		    };

		    await VerifyDiagnosticsAsync(code, new DiagnosticLocation(9, 41));
	    }
    }
}