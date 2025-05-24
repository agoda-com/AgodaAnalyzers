using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Playwright;
using NUnit.Framework;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using System.Text.RegularExpressions;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    [TestFixture]
    public class AG0046EnforceGetByTestIdUsageInPlaywrightTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0046EnforceGetByTestIdUsageInPlaywright();

        protected override string DiagnosticId => "AG0046";

        [Test]
        public async Task AG0046_WhenUsingGetByText_ShowsWarning()
        {
            var test = new CodeDescriptor
            {
                References = new[] { typeof(IPage).Assembly, typeof(Regex).Assembly },
                Code = @"
using System.Threading.Tasks;
using Microsoft.Playwright;

class TestClass
{
    public async Task TestMethod(IPage page)
    {
        await page.GetByText(""Submit"").ClickAsync();
    }
}"
            };

            var expected = new DiagnosticLocation(9, 20, "GetByText");
            await VerifyDiagnosticsAsync(test, expected);
        }

        [Test]
        public async Task AG0046_WhenUsingGetByRole_ShowsWarning()
        {
            var test = new CodeDescriptor
            {
                References = new[] { typeof(IPage).Assembly, typeof(Regex).Assembly },
                Code = @"
using System.Threading.Tasks;
using Microsoft.Playwright;

class TestClass
{
    public async Task TestMethod(IPage page)
    {
        await page.GetByRole(AriaRole.Button, new() { Name = ""Login"" }).ClickAsync();
    }
}"
            };

            var expected = new DiagnosticLocation(9, 20, "GetByRole");
            await VerifyDiagnosticsAsync(test, expected);
        }

        [Test]
        public async Task AG0046_WhenUsingGetByLabel_ShowsWarning()
        {
            var test = new CodeDescriptor
            {
                References = new[] { typeof(IPage).Assembly, typeof(Regex).Assembly },
                Code = @"
using System.Threading.Tasks;
using Microsoft.Playwright;

class TestClass
{
    public async Task TestMethod(IPage page)
    {
        await page.GetByLabel(""Username"").FillAsync(""user123"");
    }
}"
            };

            var expected = new DiagnosticLocation(9, 20, "GetByLabel");
            await VerifyDiagnosticsAsync(test, expected);
        }

        [Test]
        public async Task AG0046_WhenUsingLocator_ShowsWarning()
        {
            var test = new CodeDescriptor
            {
                References = new[] { typeof(IPage).Assembly, typeof(Regex).Assembly },
                Code = @"
using System.Threading.Tasks;
using Microsoft.Playwright;

class TestClass
{
    public async Task TestMethod(IPage page)
    {
        await page.Locator(""button.submit-btn"").ClickAsync();
    }
}"
            };

            var expected = new DiagnosticLocation(9, 20, "Locator");
            await VerifyDiagnosticsAsync(test, expected);
        }

        [Test]
        public async Task AG0046_WhenUsingGetByTestId_NoWarning()
        {
            var test = new CodeDescriptor
            {
                References = new[] { typeof(IPage).Assembly, typeof(Regex).Assembly },
                Code = @"
using System.Threading.Tasks;
using Microsoft.Playwright;

class TestClass
{
    public async Task TestMethod(IPage page)
    {
        await page.GetByTestId(""submit-button"").ClickAsync();
    }
}"
            };

            await VerifyDiagnosticsAsync(test, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0046_WhenUsingGetByRoleForAccessibility_NoWarning()
        {
            var test = new CodeDescriptor
            {
                References = new[] { typeof(IPage).Assembly, typeof(Regex).Assembly },
                Code = @"
using System.Threading.Tasks;
using Microsoft.Playwright;

class TestClass
{
    public async Task TestMethod(IPage page)
    {
        // Testing accessibility
        await page.GetByRole(AriaRole.Button, new() { Name = ""Login"" }).ClickAsync();
    }
}"
            };

            await VerifyDiagnosticsAsync(test, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0046_WhenUsingFrameLocator_ShowsWarning()
        {
            var test = new CodeDescriptor
            {
                References = new[] { typeof(IPage).Assembly, typeof(Regex).Assembly },
                Code = @"
using System.Threading.Tasks;
using Microsoft.Playwright;

class TestClass
{
    public async Task TestMethod(IPage page)
    {
        await page.FrameLocator(""#auth-frame"").GetByText(""Submit"").ClickAsync();
    }
}"
            };

            var expected = new DiagnosticLocation(9, 48, "GetByText");
            await VerifyDiagnosticsAsync(test, expected);
        }
    }
} 