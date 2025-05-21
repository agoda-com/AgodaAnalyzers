using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.CodeFixes.AgodaCustom;

[TestFixture]
public class AG0047RedundantScrollBeforeClickCodeFixProviderTests : CodeFixVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0047RedundantScrollBeforeClickAnalyzer();
    protected override CodeFixProvider CodeFixProvider => new AG0047RedundantScrollBeforeClickCodeFixProvider();
    protected override string DiagnosticId => AG0047RedundantScrollBeforeClickAnalyzer.DIAGNOSTIC_ID;

    [Test]
    public async Task CodeFix_RemovesRedundantScrollIntoViewIfNeededAsync_BeforeClickAsync()
    {
        var testCode = @"
using System.Threading.Tasks;
using Microsoft.Playwright;

class TestClass
{
    public async Task TestMethod(ILocator element)
    {
        await element.ScrollIntoViewIfNeededAsync();
        await element.ClickAsync();
    }
}";
        var fixedCode = @"
using System.Threading.Tasks;
using Microsoft.Playwright;

class TestClass
{
    public async Task TestMethod(ILocator element)
    {
        await element.ClickAsync();
    }
}";
        await VerifyCodeFixAsync(testCode, fixedCode);
    }

    [Test]
    public async Task CodeFix_DoesNotChange_WhenNoRedundantScroll()
    {
        var testCode = @"
using System.Threading.Tasks;
using Microsoft.Playwright;

class TestClass
{
    public async Task TestMethod(ILocator element)
    {
        await element.ClickAsync();
    }
}";
        await VerifyCodeFixAsync(testCode, testCode);
    }
}
