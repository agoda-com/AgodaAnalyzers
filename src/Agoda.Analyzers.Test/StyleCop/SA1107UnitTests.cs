using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Agoda.Analyzers.CodeFixes.StyleCop;
using Agoda.Analyzers.StyleCop;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.StyleCop;

/// <summary>
/// This class contains unit tests for <see cref="SA1107CodeMustNotContainMultipleStatementsOnOneLine"/> and
/// <see cref="SA1107CodeFixProvider"/>.
/// </summary>
public class SA1107UnitTests : CodeFixVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new SA1107CodeMustNotContainMultipleStatementsOnOneLine();
        
    protected override string DiagnosticId => SA1107CodeMustNotContainMultipleStatementsOnOneLine.DIAGNOSTIC_ID;
        
    protected override CodeFixProvider CodeFixProvider => new SA1107CodeFixProvider();
        
    [Test]
    public async Task TestCorrectCodeAsync()
    {
        var testCode = @"
                using System;
                class ClassName
                {
                    public static void Foo(string a, string b) 
                    {
                        int i = 5;
                        int j = 6, k = 3;
                        if(true)
                        {
                            i++;
                        }
                        else
                        {
                            j++;
                        }
                        Foo(""a"", ""b"");
                
                        Func<int, int, int> f = (c, d) => c + d;
                        Func<int, int, int> g = (c, d) => { return c + d; };
                    }
                }
            ";
            
        await VerifyDiagnosticsAsync(testCode, EmptyDiagnosticResults);
    }

    [Test]
    public async Task TestWrongCodeAsync()
    {
        var testCode = @"
using System;
class ClassName
{
    public static void Foo(string a, string b)
    {
        int i = 5; int j = 6, k = 3; if(true)
        {
            i++;
        }
        else
        {
            j++;
        } Foo(""a"", ""b"");

        Func<int, int, int> g = (c, d) => { c++; return c + d; };
    }
}
";
            
        var expected = new[]
        {
            new DiagnosticLocation(7, 20),
            new DiagnosticLocation(7, 38),
            new DiagnosticLocation(14, 11),
            new DiagnosticLocation(16, 50)
        };

        var fixedCode = @"
using System;
class ClassName
{
    public static void Foo(string a, string b)
    {
        int i = 5;
        int j = 6, k = 3;
        if (true)
        {
            i++;
        }
        else
        {
            j++;
        }

        Foo(""a"", ""b"");

        Func<int, int, int> g = (c, d) => { c++;
            return c + d; };
    }
}
";

        await VerifyDiagnosticsAsync(testCode, expected);
        await VerifyDiagnosticsAsync(fixedCode, EmptyDiagnosticResults);
        await VerifyCodeFixAsync(testCode, fixedCode);
    }

    [Test]
    public async Task TestThatAnalyzerDoesntCrashOnEmptyBlockAsync()
    {
        var testCode = @"
                using System;
                class ClassName
                {
                    public static void Foo(string a, string b)
                    {
                    }
                }
            ";
            
        await VerifyDiagnosticsAsync(testCode, EmptyDiagnosticResults);
    }
}