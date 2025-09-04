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

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(7, 53));
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

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(9, 40));
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

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(7, 48));
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

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(7, 56));
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

    // Tests for new XPath detection in declarations
    [Test]
    public async Task AG0045_WhenUsingXPathInConstDeclaration_ShowError()
    {
        var code = @"
            class TestClass
            {
                public const string XPathBaseDiscountInput = ""//*[@data-element-name=\""ycs-channel-discounts-base-discount-input\""]"";
            }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(4, 62));
    }

    [Test]
    public async Task AG0045_WhenUsingXPathInMultipleConstDeclarations_ShowMultipleErrors()
    {
        var code = @"
            class TestClass
            {
                public const string XPathStackingTypeToggle = ""//*[@data-element-name=\""ycs-channel-discounts-allow-stack-toggle\""]"";
                public const string XPathStackedDiscountInput = ""//*[@data-element-name=\""ycs-channel-discounts-stacked-discount-input\""]"";
                public const string XPathFirstRateplan = ""//*[@data-testid=\""ycs-promotion-rate-plans-v2-option-1997\""]"";
            }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation[]
        {
            new DiagnosticLocation(4, 63),
            new DiagnosticLocation(5, 65),
            new DiagnosticLocation(6, 58)
        });
    }

    [Test]
    public async Task AG0045_WhenUsingXPathInStaticReadonlyField_ShowError()
    {
        var code = @"
            class TestClass
            {
                private static readonly string XPathDeactivationFooterButton = ""//*[@data-element-name=\""ycs-channel-discounts-deactivate-channel-button\""]"";
            }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(4, 80));
    }

    [Test]
    public async Task AG0045_WhenUsingXPathInPropertyInitializer_ShowError()
    {
        var code = @"
            class TestClass
            {
                public string XPathSaveButton { get; } = ""//*[@data-testid=\""footer-save-promotion\""]"";
            }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(4, 58));
    }

    [Test]
    public async Task AG0045_WhenUsingXPathInReturnStatement_ShowError()
    {
        var code = @"
            class TestClass
            {
                public string GetXPathSelector()
                {
                    return ""//*[@data-selenium=\""save-success-toast\""]"";
                }
            }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(6, 28));
    }

    [Test]
    public async Task AG0045_WhenUsingXPathInInterpolatedString_ShowError()
    {
        var code = @"
            class TestClass
            {
                public static string XPathDatePickerDate(string date) => $""//*[@data-day=\""{date}\""]"";
            }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(4, 74));
    }

    [Test]
    public async Task AG0045_WhenUsingXPathInMethodWithInterpolation_ShowError()
    {
        var code = @"
            class TestClass
            {
                public string GetDateSelector(string date)
                {
                    return $""//*[@data-day=\""{date}\""]"";
                }
            }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(6, 28));
    }

    [Test]
    public async Task AG0045_WhenUsingXPathNamedVariableWithNonXPathValue_ShowError()
    {
        var code = @"
            class TestClass
            {
                public const string XPathSelector = ""button.submit"";
            }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(4, 53));
    }

    [Test]
    public async Task AG0045_WhenUsingNonXPathNamedVariableWithXPathValue_ShowError()
    {
        var code = @"
            class TestClass
            {
                public const string ButtonSelector = ""//*[@data-testid=\""submit-button\""]"";
            }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(4, 54));
    }

    [Test]
    public async Task AG0045_WhenUsingXPathWithDataSeleniumAttribute_ShowError()
    {
        var code = @"
            class TestClass
            {
                public const string XPathCxlPolicyOption13308 = ""//*[@data-selenium=\""channel-discount-cancellation-policy-selection-item-13308\""]"";
            }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(4, 65));
    }

    [Test]
    public async Task AG0045_WhenUsingXPathWithComplexExpression_ShowError()
    {
        var code = @"
            class TestClass
            {
                public const string ComplexXPath = ""//div[contains(@class,'menu')]/following-sibling::div[@data-testid='item']"";
            }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(4, 52));
    }

    [Test]
    public async Task AG0045_WhenUsingValidCssSelectors_NoError()
    {
        var code = @"
            class TestClass
            {
                public const string ButtonSelector = ""button.submit"";
                public const string FormSelector = "".form-container > input[type='text']"";
                public const string IdSelector = ""#submit-button"";
                public string DataTestIdSelector { get; } = ""[data-testid='login-form']"";
                
                public string GetSelector(string id)
                {
                    return $""button[data-id='{id}']"";
                }
            }";

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0045_WhenUsingValidAriaSelectors_NoError()
    {
        var code = @"
            class TestClass
            {
                public const string AriaSelector = ""[aria-label='Submit Form']"";
                public const string RoleSelector = ""[role='button']"";
                public string AccessibilitySelector { get; } = ""[aria-describedby='help-text']"";
            }";

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0045_WhenUsingMixedValidAndInvalidSelectors_ShowOnlyXPathErrors()
    {
        var code = @"
            class TestClass
            {
                public const string ValidSelector = ""button.submit"";
                public const string XPathSelector = ""//*[@data-testid='submit']"";
                public const string AnotherValidSelector = ""#login-form"";
                public const string AnotherXPathSelector = ""//div[@class='container']"";
            }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation[]
        {
            new DiagnosticLocation(5, 53),
            new DiagnosticLocation(7, 60)
        });
    }

    [Test]
    public async Task AG0045_WhenUsingXPathInVariableDeclarationWithMultipleVariables_ShowError()
    {
        var code = @"
            class TestClass
            {
                public void TestMethod()
                {
                    string validSelector = ""button.submit"", xpathSelector = ""//*[@id='test']"";
                }
            }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(6, 77));
    }
} 