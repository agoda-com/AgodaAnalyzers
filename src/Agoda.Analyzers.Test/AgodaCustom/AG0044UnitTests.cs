using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom;

[TestFixture]
[Parallelizable(ParallelScope.All)]
class AG0044UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0044ForceOptionShouldNotBeUsed();

    protected override string DiagnosticId => AG0044ForceOptionShouldNotBeUsed.DIAGNOSTIC_ID;

    [Test]
    public async Task AG0044_WhenUsingForceOptionInClickAsync_ShowWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(ILocator).Assembly },
            Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    public async Task TestMethod(ILocator locator)
                    {
                        await locator.ClickAsync(new() { Force = true });
                    }
                }"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(9, 58));
    }

    [Test]
    public async Task AG0044_WhenUsingPreDefinedForceOption_ShowWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(ILocator).Assembly },
            Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    public async Task TestMethod(ILocator locator)
                    {
                        var options = new LocatorClickOptions { Force = true };
                        await locator.ClickAsync(options);
                    }
                }"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(9, 65));
    }

    [TestCase("HoverAsync", 58, TestName = "AG0044_WhenUsingForceInHoverAsync_ShowWarning")]
    [TestCase("DblClickAsync", 61,  TestName = "AG0044_WhenUsingForceInDblClickAsync_ShowWarning")]
    [TestCase("TapAsync", 56, TestName = "AG0044_WhenUsingForceInTapAsync_ShowWarning")]
    [TestCase("CheckAsync", 58, TestName = "AG0044_WhenUsingForceInCheckAsync_ShowWarning")]
    [TestCase("UncheckAsync", 60, TestName = "AG0044_WhenUsingForceInUncheckAsync_ShowWarning")]
    public async Task AG0044_WhenUsingForceOptionWithoutParams_ShowWarning(string methodName, int column)
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(ILocator).Assembly },
            Code = $@"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {{
                    public async Task TestMethod(ILocator locator)
                    {{
                        await locator.{methodName}(new() {{ Force = true }});
                    }}
                }}"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(9, column));
    }

    [TestCase("FillAsync", "\"text\"", 65, TestName = "AG0044_WhenUsingForceInFillAsync_ShowWarning")]
    [TestCase("SelectOptionAsync", "\"option\"", 75, TestName = "AG0044_WhenUsingForceInSelectOptionAsync_ShowWarning")]
    public async Task AG0044_WhenUsingForceOptionWithParams_ShowWarning(string methodName, string parameter, int column)
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(ILocator).Assembly },
            Code = $@"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {{
                    public async Task TestMethod(ILocator locator)
                    {{
                        await locator.{methodName}({parameter}, new() {{ Force = true }});
                    }}
                }}"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(9, column));
    }

    [Test]
    public async Task AG0044_WhenNotUsingForceOption_NoWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(ILocator).Assembly },
            Code = @"
                using System.Threading.Tasks;
                using Microsoft.Playwright;

                class TestClass
                {
                    public async Task TestMethod(ILocator locator)
                    {
                        await locator.ClickAsync();
                        await locator.ClickAsync(new() { Timeout = 5000 });
                        var options = new LocatorClickOptions { Timeout = 5000 };
                        await locator.ClickAsync(options);
                    }
                }"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0044_WhenUsingCustomType_NoWarning()
    {
        var code = new CodeDescriptor
        {
            Code = @"
                using System.Threading.Tasks;

                class CustomLocator
                {
                    public async Task ClickAsync(object options) { }
                }

                class TestClass
                {
                    public async Task TestMethod()
                    {
                        var locator = new CustomLocator();
                        await locator.ClickAsync(new { Force = true });
                    }
                }"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0044_WhenSymbolIsNull_NoWarning()
    {
        var code = new CodeDescriptor
        {
            Code = @"
                using System.Threading.Tasks;

                class TestClass
                {
                    public async Task TestMethod()
                    {
                        dynamic locator = null;
                        await locator.ClickAsync(new { Force = true });
                    }
                }"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }
}