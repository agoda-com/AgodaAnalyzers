﻿using System.Threading.Tasks;
using System.Web;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    class AG0003UnitTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0003HttpContextCannotBePassedAsMethodArgument();
        
        protected override string DiagnosticId => AG0003HttpContextCannotBePassedAsMethodArgument.DIAGNOSTIC_ID;
        
        [Test]
        public async Task TestHttpContextAsArgument()
        {
	        var code = new CodeDescriptor
	        {
		        References = new[] {typeof(HttpContext).Assembly},
		        Code = @"
					using System.Web;
	
					interface ISomething {
						void SomeMethod(HttpContext c, string sampleString); // ugly interface method
					}
				
					class TestClass: ISomething {
						
						public void SomeMethod(HttpContext context, string sampleString) {
							// this method is ugly
						}
	
						public TestClass(System.Web.HttpContext context) {
							// this constructor is uglier
						}
					}"
	        };

	        var expected = new[]
	        {
		        new DiagnosticLocation(5, 23),
		        new DiagnosticLocation(10, 30),
		        new DiagnosticLocation(14, 24)
	        };
            await VerifyDiagnosticsAsync(code, expected);
        }
    }
}