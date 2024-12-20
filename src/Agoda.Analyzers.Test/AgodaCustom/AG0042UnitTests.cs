using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom;

class AG0042UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0042ElementHandlerShouldNotBeUsed();

    protected override string DiagnosticId => AG0042ElementHandlerShouldNotBeUsed.DIAGNOSTIC_ID;

    [TestCase("QuerySelectorAsync")]
    [TestCase("QuerySelectorAllAsync")]
    [TestCase("WaitForSelectorAsync")]
    public async Task AG0042_WhenUsingElementHandlerMethodWithPlaywrightPage_ShowWarning(string methodName)
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(IPage).Assembly },
            Code = $@"
                    using System.Threading.Tasks;
                    using Microsoft.Playwright;

                    class TestClass
                    {{
                        public async Task TestMethod(IPage page)
                        {{
                            await page.{methodName}(""#element"");
                        }}
                    }}"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(9, 35));
    }

    [Test]
    public async Task AG0042_WhenUsingLocatorMethod_NoWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(IPage).Assembly },
            Code = @"
                    using System.Threading.Tasks;
                    using Microsoft.Playwright;

                    class TestClass
                    {
                        public void TestMethod(IPage page)
                        {
                            var element = page.Locator(""[data-testid='element']"");
                            element.GetByRole(AriaRole.Button).ClickAsync();
                        }
                    }"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0042_WhenUsingMethodReturningElementHandleArray_ShowWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(IPage).Assembly },
            Code = @"
            using System.Threading.Tasks;
            using Microsoft.Playwright;

            class TestClass
            {
                public async Task TestMethod(IPage page)
                {
                    await page.QuerySelectorAllAsync(""#element"");
                }
            }"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(9, 27));
    }

    [Test]
    public async Task AG0042_WhenUsingMethodReturningNonTaskType_NoWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(IPage).Assembly },
            Code = @"
            using System.Threading.Tasks;
            using Microsoft.Playwright;

            class TestClass
            {
                public void TestMethod(IPage page)
                {
                    var result = page.ToString();
                }
            }"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0042_WhenUsingMethodWithGenericTaskButNotElementHandle_NoWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(IPage).Assembly },
            Code = @"
            using System.Threading.Tasks;
            using Microsoft.Playwright;

            class TestClass
            {
                public async Task TestMethod(IPage page)
                {
                    await page.EvaluateAsync<string>(""() => 'test'"");
                }
            }"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0042_WhenMethodSymbolIsNull_NoWarning()
    {
        var code = new CodeDescriptor
        {
            Code = @"
            using System.Threading.Tasks;

            class TestClass
            {
                public async Task TestMethod()
                {
                    dynamic page = null;
                    // This will make methodSymbol null since it's dynamic
                    await page.NonExistentMethod(""#element"");
                }
            }"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0042_WhenUsingQuerySelectorAsyncWithPlaywrightPage_ShowWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(IPage).Assembly },
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

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(9, 31));
    }

    [Test]
    public async Task AG0042_WhenUsingQuerySelectorAsyncWithIPageInstanceVariable_ShowWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(IPage).Assembly },
            Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    private IPage _page;

                    public async Task TestMethod()
                    {
                        await _page.QuerySelectorAsync(""#element"");
                    }
                }"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(11, 31));
    }

    [Test]
    public async Task AG0042_WhenUsingQuerySelectorAsyncWithLocalIPageVariable_ShowWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(IPage).Assembly },
            Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    public async Task TestMethod()
                    {
                        IPage page = null;
                        await page.QuerySelectorAsync(""#element"");
                    }
                }"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(10, 31));
    }

    [Test]
    public async Task AG0042_WhenUsingQuerySelectorAsyncWithIPageProperty_ShowWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(IPage).Assembly },
            Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    public IPage Page { get; set; }

                    public async Task TestMethod()
                    {
                        await Page.QuerySelectorAsync(""#element"");
                    }
                }"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(11, 31));
    }

    [Test]
    public async Task AG0042_WhenUsingQuerySelectorAsyncWithNonIPageType_NoWarning()
    {
        var code = new CodeDescriptor
        {
            // No need to reference Microsoft.Playwright
            Code = @"
                using System.Threading.Tasks;

                class CustomPage
                {
                    public async Task QuerySelectorAsync(string selector) { }
                }

                class TestClass
                {
                    public async Task TestMethod()
                    {
                        CustomPage page = new CustomPage();
                        await page.QuerySelectorAsync(""#element"");
                    }
                }"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0042_WhenUsingLocatorMethodName_NoWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(IPage).Assembly },
            Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    public void TestMethod(IPage page)
                    {
                        page.Locator(""#selector"");
                    }
                }"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0042_WhenSymbolIsNull_NoWarning()
    {
        var code = new CodeDescriptor
        {
            // Intentionally use an unknown variable to cause symbol to be null
            Code = @"
                using System.Threading.Tasks;

                class TestClass
                {
                    public async Task TestMethod()
                    {
                        dynamic unknownVariable = null;
                        await unknownVariable.QuerySelectorAsync(""#element"");
                    }
                }"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0042_WhenTypeSymbolIsNull_NoWarning()
    {
        var code = new CodeDescriptor
        {
            Code = @"
                using System.Threading.Tasks;

                class TestClass
                {
                    public async Task TestMethod(dynamic page)
                    {
                        await page.QuerySelectorAsync(""#element"");
                    }
                }"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0042_WhenInvocationExpressionIsNotMemberAccess_NoWarning()
    {
        var code = new CodeDescriptor
        {
            Code = @"
                using System.Threading.Tasks;

                class TestClass
                {
                    public async Task TestMethod()
                    {
                        await QuerySelectorAsync(""#element"");
                    }

                    public async Task QuerySelectorAsync(string selector) { }
                }"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0042_WhenMemberAccessExpressionHasNoIdentifier_NoWarning()
    {
        var code = new CodeDescriptor
        {
            Code = @"
                using System.Threading.Tasks;

                class TestClass
                {
                    public async Task TestMethod()
                    {
                        var func = GetPage();
                        await func().QuerySelectorAsync(""#element"");
                    }

                    public System.Func<dynamic> GetPage() => null;
                }"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }
}