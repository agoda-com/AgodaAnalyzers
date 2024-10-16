using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom;

class AG0042UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0042QuerySelectorShouldNotBeUsed();

    protected override string DiagnosticId => AG0042QuerySelectorShouldNotBeUsed.DIAGNOSTIC_ID;

    [Test]
    public async Task AG0042_WhenUsingQuerySelectorAsyncWithPlaywrightPage_ShowWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] {typeof(IPage).Assembly},
            Code = @"
                    using System.Threading.Tasks;
                    using Microsoft.Playwright;

                    class TestClass
                    {
                        public async Task TestMethod(IPage page)
                        {
                            await page.QuerySelectorAsync(""#element"");
                        }
                    }"
        };
        
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(9, 35));
    }
    
    [Test]
    public async Task AG0042_RandomPageClass_DoNotShowWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] {typeof(IPage).Assembly},
            Code = @"
                        using System.Threading.Tasks;

                        class TestClass
                        {
                            public async Task TestMethod(CustomPage page)
                            {
                                await page.QuerySelectorAsync(""#element"");
                            }
                        }

                        class CustomPage
                        {
                            public Task QuerySelectorAsync(string selector) => Task.CompletedTask;
                        }"
        };
        
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }
}
