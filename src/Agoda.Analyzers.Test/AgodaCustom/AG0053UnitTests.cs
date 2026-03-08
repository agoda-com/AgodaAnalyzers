using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    [TestFixture]
    internal class AG0053UnitTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0053ScreenshotMustHavePrecedingWait();

        protected override string DiagnosticId => AG0053ScreenshotMustHavePrecedingWait.DIAGNOSTIC_ID;

        [Test]
        public async Task AG0053_PageScreenshotWithoutWait_ShowsWarning()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(Microsoft.Playwright.IPage).Assembly },
                Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    public async Task TestMethod(IPage page)
                    {
                        await page.GotoAsync(""/dashboard"");
                        await page.ScreenshotAsync();
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(10, 31));
        }

        [Test]
        public async Task AG0053_ScreenshotWithPrecedingWaitForSelectorAsync_NoWarning()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(Microsoft.Playwright.IPage).Assembly },
                Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    public async Task TestMethod(IPage page)
                    {
                        await page.GotoAsync(""/dashboard"");
                        await page.WaitForSelectorAsync("".content-loaded"");
                        await page.ScreenshotAsync();
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0053_WaitForLoadStateIsNotSufficient_ShowsWarning()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(Microsoft.Playwright.IPage).Assembly },
                Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    public async Task TestMethod(IPage page)
                    {
                        await page.GotoAsync(""/profile"");
                        await page.WaitForLoadStateAsync();
                        await page.ScreenshotAsync();
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(11, 31));
        }

        [Test]
        public async Task AG0053_NonPlaywrightScreenshotAsync_NoWarning()
        {
            var code = new CodeDescriptor
            {
                Code = @"
                using System.Threading.Tasks;

                class CustomPage
                {
                    public Task ScreenshotAsync() => Task.CompletedTask;
                }

                class TestClass
                {
                    public async Task TestMethod()
                    {
                        var page = new CustomPage();
                        await page.ScreenshotAsync();
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0053_ScreenshotAssignedToVar_ShowsWarning()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(Microsoft.Playwright.IPage).Assembly },
                Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    public async Task TestMethod(IPage page)
                    {
                        await page.GotoAsync(""/settings"");
                        var bytes = await page.ScreenshotAsync();
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(10, 43));
        }

        [Test]
        public async Task AG0053_ScreenshotWithWaitForSelectorBefore_NoWarning()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(Microsoft.Playwright.IPage).Assembly },
                Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    public async Task TestMethod(IPage page)
                    {
                        await page.GotoAsync(""/analytics"");
                        var element = await page.WaitForSelectorAsync("".chart-ready"");
                        var bytes = await page.ScreenshotAsync();
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0053_MultiplePageScreenshots_FirstWithoutWait_ShowsWarning()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(Microsoft.Playwright.IPage).Assembly },
                Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    public async Task TestMethod(IPage page)
                    {
                        await page.GotoAsync(""/dashboard"");
                        await page.ScreenshotAsync();
                        await page.WaitForSelectorAsync("".loaded"");
                        await page.ScreenshotAsync();
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(10, 31));
        }
    }
}
