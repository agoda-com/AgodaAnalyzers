using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom;

class AG0047UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0047RedundantScrollBeforeClickAnalyzer();
    protected override string DiagnosticId => AG0047RedundantScrollBeforeClickAnalyzer.DIAGNOSTIC_ID;

    [Test]
    public async Task AG0047_WhenScrollAndClickOnSameLocator_ShouldShowDiagnostic()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(ILocator).Assembly },
            Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    public async Task TestMethod(ILocator element)
                    {
                        await element.ScrollIntoViewIfNeededAsync();
                        await element.ClickAsync();
                    }
                }"
        };
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(9, 25));
    }

    [Test]
    public async Task AG0047_WhenScrollAndClickWithDelay_ShouldShowDiagnostic()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(ILocator).Assembly },
            Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    public async Task TestMethod(ILocator element)
                    {
                        await element.ScrollIntoViewIfNeededAsync();
                        await Task.Delay(100);
                        await element.ClickAsync();
                    }
                }"
        };
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(9, 25));
    }

    [Test]
    public async Task AG0047_WhenOnlyClick_ShouldNotShowDiagnostic()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(ILocator).Assembly },
            Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    public async Task TestMethod(ILocator element)
                    {
                        await element.ClickAsync();
                    }
                }"
        };
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation[0]);
    }

    [Test]
    public async Task AG0047_WhenScrollAndHoverThenClick_ShouldNotShowDiagnostic()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(ILocator).Assembly },
            Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    public async Task TestMethod(ILocator element)
                    {
                        await element.ScrollIntoViewIfNeededAsync();
                        await element.HoverAsync();
                        await element.ClickAsync();
                    }
                }"
        };
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation[0]);
    }
}
