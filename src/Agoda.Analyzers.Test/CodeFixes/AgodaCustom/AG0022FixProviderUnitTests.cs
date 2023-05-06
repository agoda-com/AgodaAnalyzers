using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.CodeFixes.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.CodeFixes.AgodaCustom;

class AG0022RemoveSyncMethodUnitTests : CodeFixVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0022DoNotExposeBothSyncAndAsyncVersionsOfMethods();
        
    protected override string DiagnosticId => AG0022DoNotExposeBothSyncAndAsyncVersionsOfMethods.DIAGNOSTIC_ID;
        
    protected override CodeFixProvider CodeFixProvider => new AG0022RemoveSyncMethodFixProvider();

    [Test]
    public async Task AG0022_ForInterface_ShouldRemoveSyncVersion()
    {
        var code = @"
                using System.Threading.Tasks;
                
                interface TestInterface
                {
                    bool TestMethod(string url);
                    Task<bool> TestMethodAsync(string url);
                }
            ";

        var result = @"
                using System.Threading.Tasks;
                
                interface TestInterface
                {
                    Task<bool> TestMethodAsync(string url);
                }
            ";
            
        await VerifyCodeFixAsync(code, result);
    }
        
    [Test]
    public async Task AG0022_ForClass_ShouldRemoveSyncVersion()
    {
        var code = @"
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

        var result = @"
                using System.Threading.Tasks;
                
                class TestClass
                {
                    public Task<bool> TestMethodAsync(string url) 
                    { 
                        return Task.FromResult(true);
                    }
                }
            ";
            
        await VerifyCodeFixAsync(code, result);
    }
        
    [Test]
    public async Task AG0022_ForMethodWithComment_ShouldRemoveSyncVersionWithComment()
    {
        var code = @"
                using System.Threading.Tasks;
                
                interface TestInterface
                {
                    /// <summary>
                    /// This is a summary for TestMethod
                    /// </summary>
                    bool TestMethod(string url); // comment
                
                    /// <summary>
                    /// This is a summary for TestMethod
                    /// </summary>
                    Task<bool> TestMethodAsync(string url); // comment
                }
            ";

        var result = @"
                using System.Threading.Tasks;
                
                interface TestInterface
                {
                
                    /// <summary>
                    /// This is a summary for TestMethod
                    /// </summary>
                    Task<bool> TestMethodAsync(string url); // comment
                }
            ";
            
        await VerifyCodeFixAsync(code, result);
    }
}