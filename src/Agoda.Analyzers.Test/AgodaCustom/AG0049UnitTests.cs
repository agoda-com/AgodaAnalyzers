using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    [TestFixture]
    internal class AG0049UnitTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0049AvoidWaitForResponseAsync();
        
        protected override string DiagnosticId => AG0049AvoidWaitForResponseAsync.DIAGNOSTIC_ID;

        [Test]
        public async Task AG0049_WhenUsingWaitForResponseAsync_ShowError()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(Microsoft.Playwright.IPage).Assembly, typeof(System.Text.RegularExpressions.Regex).Assembly },
                Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    public async Task TestMethod(IPage page)
                    {
                        var response = await page.WaitForResponseAsync(response => response.Url.Contains(""api/test""));
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(9, 46));
        }

        [Test]
        public async Task AG0049_WhenUsingWaitForResponseAsyncWithComplexPredicate_ShowError()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(Microsoft.Playwright.IPage).Assembly, typeof(System.Text.RegularExpressions.Regex).Assembly },
                Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    public async Task TestMethod(IPage page, string elementSelector)
                    {
                        var response = await page.WaitForResponseAsync(response => response.Url.Contains(elementSelector));
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(9, 46));
        }

        [Test]
        public async Task AG0049_WhenUsingWaitForResponseAsyncWithStringUrl_ShowError()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(Microsoft.Playwright.IPage).Assembly, typeof(System.Text.RegularExpressions.Regex).Assembly },
                Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    public async Task TestMethod(IPage page)
                    {
                        var response = await page.WaitForResponseAsync(""**/api/create"");
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(9, 46));
        }

        [Test]
        public async Task AG0049_WhenUsingWaitForResponseAsyncWithRegex_ShowError()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(Microsoft.Playwright.IPage).Assembly, typeof(System.Text.RegularExpressions.Regex).Assembly },
                Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;
                using System.Text.RegularExpressions;

                class TestClass
                {
                    public async Task TestMethod(IPage page)
                    {
                        var regex = new Regex(@""api\/\w+"");
                        var response = await page.WaitForResponseAsync(regex);
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(11, 46));
        }

        [Test]
        public async Task AG0049_WhenUsingRunAndWaitForResponseAsync_NoError()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(Microsoft.Playwright.IPage).Assembly, typeof(System.Text.RegularExpressions.Regex).Assembly },
                Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    public async Task TestMethod(IPage page)
                    {
                        var response = await page.RunAndWaitForResponseAsync(async () =>
                        {
                            await page.ClickAsync(""button"");
                        }, response => response.Url.Contains(""api/test""));
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0049_WhenUsingRunAndWaitForResponseAsyncWithComplexAction_NoError()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(Microsoft.Playwright.IPage).Assembly, typeof(System.Text.RegularExpressions.Regex).Assembly, typeof(System.Text.Json.JsonElement).Assembly },
                Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;
                using System.Linq;

                class TestClass
                {
                    public async Task TestMethod(IPage page, string[] selectedCustomerSegments, string RequestBulkCreatePromotion)
                    {
                        var response = await page.RunAndWaitForResponseAsync(async () =>
                        {
                            await ClickCreateButton();
                        }, response =>
                        {
                            var requestBody = response.Request.PostDataJSON().ToString();
                            return response.Url.Contains(RequestBulkCreatePromotion) &&
                                   selectedCustomerSegments.All(id => requestBody.Contains(""CustomerSegmentGroupId""));
                        });
                    }

                    private async Task ClickCreateButton() { }
                }"
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0049_WhenUsingOtherPageMethods_NoError()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(Microsoft.Playwright.IPage).Assembly, typeof(System.Text.RegularExpressions.Regex).Assembly },
                Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    public async Task TestMethod(IPage page)
                    {
                        await page.ClickAsync(""button"");
                        await page.FillAsync(""input"", ""value"");
                        var element = await page.WaitForSelectorAsync("".my-element"");
                        var request = await page.WaitForRequestAsync(""**/api/test"");
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0049_WhenUsingWaitForResponseAsyncOnNonPlaywrightObject_NoError()
        {
            var code = new CodeDescriptor
            {
                Code = @"
                using System.Threading.Tasks;

                class CustomPage
                {
                    public async Task<object> WaitForResponseAsync(string url) => null;
                }

                class TestClass
                {
                    public async Task TestMethod()
                    {
                        var customPage = new CustomPage();
                        var response = await customPage.WaitForResponseAsync(""api/test"");
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0049_WhenUsingWaitForResponseAsyncInVariableAssignment_ShowError()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(Microsoft.Playwright.IPage).Assembly, typeof(System.Text.RegularExpressions.Regex).Assembly },
                Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    public async Task TestMethod(IPage page)
                    {
                        IResponse response;
                        response = await page.WaitForResponseAsync(resp => resp.Status == 200);
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(10, 42));
        }

        [Test]
        public async Task AG0049_WhenUsingWaitForResponseAsyncWithTimeout_ShowError()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(Microsoft.Playwright.IPage).Assembly, typeof(System.Text.RegularExpressions.Regex).Assembly },
                Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    public async Task TestMethod(IPage page)
                    {
                        var options = new PageWaitForResponseOptions { Timeout = 5000 };
                        var response = await page.WaitForResponseAsync(""**/api/test"", options);
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(10, 46));
        }
    }
}
