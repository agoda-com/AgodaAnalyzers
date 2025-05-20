using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
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
            References = new[] {typeof(IHttpContextAccessor).Assembly},
            Code = @"
using Microsoft.AspNetCore.Http;
					class TestClass {
						public void TestMethod(IHttpContextAccessor httpCtx) {
							var queryString = httpCtx.HttpContext.Request.QueryString;
						}
					}"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(5, 54));
    }

    [Test]
    public async Task AG0011_ThroughLocalVariable_ShowsWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] {typeof(IHttpContextAccessor).Assembly},
            Code = @"
using Microsoft.AspNetCore.Http;
					class TestClass {
						public void TestMethod(IHttpContextAccessor httpCtx) {
							var request = httpCtx.HttpContext.Request;
							var queryParamValue = request.QueryString.Value;
						}
					}"
        };
        
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(6, 38));
    }
}