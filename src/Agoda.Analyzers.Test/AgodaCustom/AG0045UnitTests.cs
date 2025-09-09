using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom;

[TestFixture]
class AG0045UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0045XPathShouldNotBeUsedInPlaywrightLocators();

    protected override string DiagnosticId => AG0045XPathShouldNotBeUsedInPlaywrightLocators.DIAGNOSTIC_ID;

    [Test]
    public async Task AG0045_WhenUsingXPathInLocator_ShowError()
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
                        await page.Locator(""//div[@class='form']/button"").ClickAsync();
                    }
                }"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(9, 44));
    }

    [Test]
    public async Task AG0045_WhenUsingXPathPrefix_ShowError()
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
                        await page.Locator(""xpath=//button[@id='submit']"").ClickAsync();
                    }
                }"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(9, 44));
    }

    [Test]
    public async Task AG0045_WhenUsingXPathInVariable_ShowError()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(IPage).Assembly, typeof(Regex).Assembly },
            Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    private const string Selector = ""//div[contains(@class,'menu')]/following-sibling::div"";

                    public async Task TestMethod(IPage page)
                    {
                        await page.Locator(Selector).ClickAsync();
                    }
                }"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(11, 44));
    }

    [Test]
    public async Task AG0045_WhenUsingXPathInLocalVariable_ShowError()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(IPage).Assembly, typeof(Regex).Assembly },
            Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    public async Task TestMethod(IPage page)
                    {
                        string myVar = ""//div[@class='form']/button"";
                        await page.Locator(myVar).ClickAsync();
                    }
                }"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(10, 44));
    }

    [Test]
    public async Task AG0045_WhenUsingXPathInField_ShowError()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(IPage).Assembly, typeof(Regex).Assembly },
            Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    private string _selector = ""//div[@class='form']/button"";

                    public async Task TestMethod(IPage page)
                    {
                        await page.Locator(_selector).ClickAsync();
                    }
                }"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(11, 44));
    }

    [Test]
    public async Task AG0045_WhenUsingXPathInProperty_ShowError()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(IPage).Assembly, typeof(Regex).Assembly },
            Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    private string Selector { get; } = ""//div[@class='form']/button"";

                    public async Task TestMethod(IPage page)
                    {
                        await page.Locator(Selector).ClickAsync();
                    }
                }"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(11, 44));
    }

    [Test]
    public async Task AG0045_WhenUsingXPathWithAxes_ShowError()
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
                        await page.Locator(""//div[contains(@class,'menu')]/following-sibling::div"").ClickAsync();
                    }
                }"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(9, 44));
    }

    [Test]
    public async Task AG0045_WhenUsingRecommendedLocators_NoError()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(IPage).Assembly, typeof(Regex).Assembly },
            Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    public async Task TestMethod(IPage page)
                    {
                        await page.GetByRole(AriaRole.Button, new() { Name = ""Submit"" }).ClickAsync();
                        await page.GetByText(""Sign Up"").ClickAsync();
                        await page.GetByTestId(""submit-button"").ClickAsync();
                        await page.GetByLabel(""Username"").FillAsync(""user123"");
                        await page.Locator("".submit-container > button.primary"").ClickAsync();
                    }
                }"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0045_WhenUsingNonPlaywrightLocator_NoError()
    {
        var code = new CodeDescriptor
        {
            Code = @"
                using System.Threading.Tasks;

                class CustomLocator
                {
                    public async Task ClickAsync(string selector) { }
                }

                class TestClass
                {
                    public async Task TestMethod()
                    {
                        var locator = new CustomLocator();
                        await locator.ClickAsync(""//div[@class='form']/button"");
                    }
                }"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0045_WhenUsingDollarSignInLogging_NoError()
    {
        var code = new CodeDescriptor
        {
            Code = @"
                using System.Threading.Tasks;
                using System.Linq;

                class LogHelper
                {
                    public static string GetCommaSeparatedList(System.Collections.Generic.IEnumerable<int> items)
                    {
                        return string.Join("","", items);
                    }
                }

                class GwModel
                {
                    public System.Collections.Generic.List<RateMutation> RatesMutationList { get; set; }
                }

                class RateMutation
                {
                    public int RatePlanId { get; set; }
                }

                class Logger
                {
                    public void Information(string message) { }
                }

                class TestClass
                {
                    private Logger _logger = new Logger();

                    public void TestMethod()
                    {
                        var gwModel = new GwModel();
                        _logger.Information($""Updating mutation for A variant "" +
                                            $""${LogHelper.GetCommaSeparatedList(gwModel
                                                .RatesMutationList?
                                                .Select(x => x.RatePlanId))}"");
                    }
                }"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0045_WhenUsingDoubleSlashInNonXPathContext_NoError()
    {
        var code = new CodeDescriptor
        {
            Code = @"
                class TestClass
                {
                    public void TestMethod()
                    {
                        string url = ""https://example.com//path"";
                        string comment = ""// This is a comment"";
                        string regex = ""//d+"";
                    }
                }"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }
} 