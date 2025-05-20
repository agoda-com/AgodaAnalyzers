using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom;

[TestFixture]
internal class AG0022UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0022DoNotExposeBothSyncAndAsyncVersionsOfMethods();
        
    protected override string DiagnosticId => AG0022DoNotExposeBothSyncAndAsyncVersionsOfMethods.DIAGNOSTIC_ID;
        
    [Test]
    public async Task AG0022_WhenNotExistBothSyncAndAsyncVersionsOnInterface_ShouldNotShowWarning()
    {
        const string code = @"
using System.Threading.Tasks;

interface TestInterface
{
    bool TestMethod(string url);
    Task<bool> NotTestMethodAsync(string url);
}
";

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }
        
    [Test]
    public async Task AG0022_WhenNotExistBothSyncAndAsyncVersionsOnClass_ShouldNotShowWarning()
    {
        const string code = @"
using System.Threading.Tasks;

class TestClass
{
    public bool TestMethod(string url)
    {
        return true;
    }
    public Task<bool> NotTestMethodAsync(string url) 
    { 
        return Task.FromResult(true);
    }
}
";

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }
        
    [Test]
    public async Task AG0022_WhenExistBothSyncAndAsyncVersionsOfMethodsOnInterface_ShouldShowWarning()
    {
        const string code = @"
using System.Threading.Tasks;

interface Interface
{
    bool TestMethod(string url);
    Task<bool> TestMethodAsync(string url);
}
			";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(6, 5));
    }

    [Test]
    public async Task AG0022_WhenExistBothSyncAndAsyncVersionsOnClass_ShouldShowWarning()
    {
        const string code = @"
using System.Threading.Tasks;

class TestClass
{
    public bool TestMethod(string url)
    {
        return true;
    }
    public Task<bool> TestMethodAsync(string url) 
    { 
        return Task.FromResult(true); 
    }
}
";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(6, 5));
    }
        
    [Test]
    public async Task AG0022_WhenExistBothSyncAndAsyncVoidVersions_ShouldShowWarning()
    {
        const string code = @"
using System.Threading.Tasks;

class TestClass
{
    public void TestMethod(string url)
    {
        return;
    }
    public async void TestMethodAsync(string url) 
    { 
        
    }
}
";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(6, 5));
    }
        
    [Test]
    public async Task AG0022_WhenExistBothSyncAndAsyncMethodsNamedSame_ShouldShowWarning()
    {
        const string code = @"
using System.Threading.Tasks;

class TestClass
{
    public bool TestMethod(string url)
    {
        return true;
    }
    public Task<bool> TestMethod(int something) 
    { 
        return Task.FromResult(true); 
    }
}
";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(6, 5));
    }
        
    [Test]
    public async Task AG0022_WhenExistBothSyncAndAsyncVersionsOfInternalMethods_ShouldNotShowWarning()
    {
        const string code = @"
using System.Threading.Tasks;

class TestClass
{
    internal bool TestMethod(string url)
    {
        return true;
    }
    internal Task<bool> TestMethodAsync(string url) 
    { 
        return Task.FromResult(true); 
    }
}
";
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }
}