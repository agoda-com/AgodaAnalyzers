using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom;

[TestFixture]
class AG0011UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0011NoDirectQueryStringAccess();
        
    protected override string DiagnosticId => AG0011NoDirectQueryStringAccess.DIAGNOSTIC_ID;
        
    [Test]
    public async Task AG0011_WithDirectAccess_ShowsWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] {typeof(HttpContext).Assembly},
            Code = @"
					class TestClass {
						public void TestMethod() {
							var queryString = System.Web.HttpContext.Current.Request.QueryString;
						}
					}"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(4, 65));
    }

    [Test]
    public async Task AG0011_ThroughLocalVariable_ShowsWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] {typeof(HttpContext).Assembly},
            Code = @"
					class TestClass {
						public void TestMethod() {
							var request = System.Web.HttpContext.Current.Request;
							var queryParamValue = request.QueryString[""queryParam""];
						}
					}"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(5, 38));
    }
}