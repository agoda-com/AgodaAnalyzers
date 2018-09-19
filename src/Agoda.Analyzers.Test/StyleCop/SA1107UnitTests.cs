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

namespace Agoda.Analyzers.Test.StyleCop
{
    /// <summary>
    /// This class contains unit tests for <see cref="SA1107CodeMustNotContainMultipleStatementsOnOneLine"/> and
    /// <see cref="SA1107CodeFixProvider"/>.
    /// </summary>
    public class SA1107UnitTests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer => new SA1107CodeMustNotContainMultipleStatementsOnOneLine();
        
        protected override string DiagnosticId => SA1107CodeMustNotContainMultipleStatementsOnOneLine.DIAGNOSTIC_ID;
        
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
            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
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
                CSharpDiagnostic().WithLocation(7, 20),
                CSharpDiagnostic().WithLocation(7, 38),
                CSharpDiagnostic().WithLocation(14, 11),
                CSharpDiagnostic().WithLocation(16, 50)
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

            await VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
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
            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task TestThatAnalyzerIgnoresStatementsWithMissingTokenAsync()
        {
            var testCode = @"
using System;
class ClassName
{
    public static void Foo(string a, string b)
    {
        int i
        if (true)
        {
            Console.WriteLine(""Bar"");
        }
    }
}
";
            var expected = new DiagnosticResult
            {
                Id = "CS1002",
                Message = "; expected",
                Severity = DiagnosticSeverity.Error
            };

            expected = expected.WithLocation(7, 14);

            await VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new SA1107CodeFixProvider();
        }
    }
}