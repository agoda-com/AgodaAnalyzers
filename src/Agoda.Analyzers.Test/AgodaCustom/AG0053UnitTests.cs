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
        public async Task AG0053_ScreenshotWithoutWait_ShowsWarning()
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

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(9, 25));
        }

        [Test]
        public async Task AG0053_ToHaveScreenshotWithoutWait_ShowsWarning()
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
                        await Assertions.Expect(page).ToHaveScreenshotAsync();
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(9, 25));
        }

        [Test]
        public async Task AG0053_ScreenshotWithPrecedingToBeVisibleAsync_NoWarning()
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
                        await Assertions.Expect(page.GetByTestId(""content"")).ToBeVisibleAsync();
                        await page.ScreenshotAsync();
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0053_ToHaveScreenshotWithPrecedingToBeVisibleAsync_NoWarning()
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
                        await Assertions.Expect(page.GetByTestId(""loaded"")).ToBeVisibleAsync();
                        await Assertions.Expect(page).ToHaveScreenshotAsync();
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
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
        public async Task AG0053_ScreenshotWithPrecedingLocatorWaitForAsync_NoWarning()
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
                        var locator = page.GetByTestId(""content"");
                        await locator.WaitForAsync();
                        await page.ScreenshotAsync();
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0053_LocatorScreenshotWithoutWait_ShowsWarning()
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
                        await page.GetByTestId(""open-modal"").ClickAsync();
                        var modal = page.GetByTestId(""modal-container"");
                        await modal.ScreenshotAsync();
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(10, 25));
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
                        await Assertions.Expect(page).ToHaveScreenshotAsync();
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(10, 25));
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
        public async Task AG0053_MultipleScreenshots_OnlyFirstWithoutWait_ShowsWarning()
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
                        await Assertions.Expect(page.GetByTestId(""content"")).ToBeVisibleAsync();
                        await Assertions.Expect(page).ToHaveScreenshotAsync();
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(9, 25));
        }
    }
}
