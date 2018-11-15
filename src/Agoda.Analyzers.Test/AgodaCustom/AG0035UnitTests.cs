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
    class AG0035UnitTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0035PreventUseOfMachineName();
        
        protected override string DiagnosticId => AG0035PreventUseOfMachineName.DIAGNOSTIC_ID;
        
	    [Test]
	    public async Task AG0035_WithMachineName_ShowsWarning()
	    {
		    var code = @"
				using System;

				namespace Test 
				{
					class TestClass
					{
						public void TestMethod() 
						{
							var machineName = Environment.MachineName;
						}
					}
				}";

		    await VerifyDiagnosticsAsync(code, new DiagnosticLocation(10, 38));
	    }
    }
}