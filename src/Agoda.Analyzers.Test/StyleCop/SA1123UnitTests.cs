using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Agoda.Analyzers.StyleCop;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.StyleCop;

/// <summary>
/// This class contains unit tests for <see cref="SA1123DoNotPlaceRegionsWithinElements"/> and
/// <see cref="RemoveRegionCodeFixProvider"/>.
/// </summary>
public class SA1123UnitTests : CodeFixVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new SA1123DoNotPlaceRegionsWithinElements();
        
    protected override string DiagnosticId => SA1123DoNotPlaceRegionsWithinElements.DIAGNOSTIC_ID;
        
    protected override CodeFixProvider CodeFixProvider => new RemoveRegionCodeFixProvider();
        
    [Test]
    public async Task TestRegionInMethodAsync()
    {
        var testCode = @"
public class Foo
{
    public void Bar()
    {
#region Foo
        string test = """";
#endregion
    }
}
";

        await VerifyDiagnosticsAsync(testCode, new DiagnosticLocation(6, 1));

        var fixedCode = @"
public class Foo
{
    public void Bar()
    {
        string test = """";
    }
}
";

        await VerifyCodeFixAsync(testCode, fixedCode);
        await VerifyCSharpFixAllFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None);
    }

    [Test]
    public async Task TestRegionPartialyInMethodAsync()
    {
        var testCode = @"
                public class Foo
                {
                    public void Bar()
                    {
                #region Foo
                        string test = """";
                    }
                #endregion
                }
            ";

        await VerifyDiagnosticsAsync(testCode, EmptyDiagnosticResults);
    }

    [Test]
    public async Task TestRegionPartialyInMethod2Async()
    {
        var testCode = @"
                public class Foo
                {
                    public void Bar()
                #region Foo
                    {
                        string test = """";
                    }
                #endregion
                }
            ";
            
        await VerifyDiagnosticsAsync(testCode, EmptyDiagnosticResults);
    }

    [Test]
    public async Task TestRegionPartialyMultipleMethodsAsync()
    {
        var testCode = @"
                public class Foo
                {
                    public void Bar()
                    {
                #region Foo
                        string test = """";
                    }
                    public void FooBar()
                    {
                        string test = """";
                #endregion
                    }
                }
            ";
            
        await VerifyDiagnosticsAsync(testCode, EmptyDiagnosticResults);
    }

    [Test]
    public async Task TestEndRegionInMethodAsync()
    {
        var testCode = @"
                public class Foo
                {
                #region Foo
                    public void Bar()
                    {
                        string test = """";
                #endregion
                    }
                }
            ";

        await VerifyDiagnosticsAsync(testCode, EmptyDiagnosticResults);
    }

    [Test]
    public async Task TestRegionOutsideMethodAsync()
    {
        var testCode = @"
                public class Foo
                {
                #region Foo
                #endregion
                    public void Bar()
                    {
                        string test = """";
                    }
                }
            ";

        await VerifyDiagnosticsAsync(testCode, EmptyDiagnosticResults);
    }

    [Test]
    public async Task TestRegionOutsideMethod2Async()
    {
        var testCode = @"
                public class Foo
                {
                #region Foo
                    public void Bar()
                    {
                        string test = """";
                    }
                #endregion
                }
            ";

        await VerifyDiagnosticsAsync(testCode, EmptyDiagnosticResults);
    }

    [Test]
    public async Task TestFixAllProviderAsync()
    {
        var testCode = @"
class ClassName
{
    void MethodName()
    {
        #region Foo
        #region Foo
        #region Foo
        #endregion
        #endregion
        #endregion
        #region Foo
        #region Foo
        #region Foo
        // Test
        #endregion
        #endregion
        #endregion
    }
}
";

        var fixedCode = @"
class ClassName
{
    void MethodName()
    {
        // Test
    }
}
";
            
        await VerifyCodeFixAsync(testCode, fixedCode);
        await VerifyCSharpFixAllFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None);
    }
}