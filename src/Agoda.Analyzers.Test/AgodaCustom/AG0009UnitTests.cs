using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom;

[TestFixture]
[Parallelizable(ParallelScope.All)]
class AG0009UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0009IHttpContextAccessorCannotBePassedAsMethodArgument();

    protected override string DiagnosticId => AG0009IHttpContextAccessorCannotBePassedAsMethodArgument.DIAGNOSTIC_ID;

    [Test]
    public async Task TestIHttpContextAccessorAsArgument()
    {
        var code = new CodeDescriptor
        {
            References = new[] {typeof(IHttpContextAccessor).Assembly, typeof(HttpContextAccessor).Assembly},
            Code = @"
using Microsoft.AspNetCore.Http;

interface ISomething
{
    void SomeMethod(IHttpContextAccessor c, string sampleString); // ugly interface method
    void SomeMethod(HttpContextAccessor c, string sampleString); // ugly interface method
}

class TestClass : ISomething
{
    public void SomeMethod(IHttpContextAccessor context, string sampleString)
    {
        // this method is ugly
    }

    public void SomeMethod(HttpContextAccessor context, string sampleString)
    {
        //this method is ugly
    }

     public TestClass(Microsoft.AspNetCore.Http.IHttpContextAccessor context)
    {
        // this constructor is uglier
    }

    public TestClass(Microsoft.AspNetCore.Http.HttpContextAccessor context)
    {
        // this constructor is uglier
    }
}"
        };

        var expected = new[]
        {
            new DiagnosticLocation(6, 21),
            new DiagnosticLocation(7, 21),
            new DiagnosticLocation(12, 28),
            new DiagnosticLocation(17, 28),
            new DiagnosticLocation(22, 23),
            new DiagnosticLocation(27, 22)
        };
        await VerifyDiagnosticsAsync(code, expected);
    }
}