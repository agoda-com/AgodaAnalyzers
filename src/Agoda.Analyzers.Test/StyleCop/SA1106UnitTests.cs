using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Agoda.Analyzers.CodeFixes.StyleCop;
using Agoda.Analyzers.StyleCop;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.StyleCop
{
    public class SA1106UnitTests : CodeFixVerifier
    {
        [Test]
        [TestCase("if (true)")]
        [TestCase("if (true) { } else")]
        [TestCase("for (int i = 0; i < 10; i++)")]
        [TestCase("while (true)")]
        public async Task TestEmptyStatementAsBlockAsync(string controlFlowConstruct)
        {
            var testCode = $@"
class TestClass
{{
    public void TestMethod()
    {{
        {controlFlowConstruct}
            ;
    }}
}}";
            var fixedCode = $@"
class TestClass
{{
    public void TestMethod()
    {{
        {controlFlowConstruct}
        {{
        }}
    }}
}}";

            var expected = CSharpDiagnostic().WithLocation(7, 13);

            await VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task TestEmptyStatementAsBlockInDoWhileAsync()
        {
            var testCode = @"
class TestClass
{
    public void TestMethod()
    {
        do
            ;
        while (false);
    }
}";
            var fixedCode = @"
class TestClass
{
    public void TestMethod()
    {
        do
        {
        }
        while (false);
    }
}";

            var expected = CSharpDiagnostic().WithLocation(7, 13);

            await VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task TestEmptyStatementWithinBlockAsync()
        {
            var testCode = @"
class TestClass
{
    public void TestMethod()
    {
        for (int i = 0; i < 10; i++)
        {
            var temp = i;
            ;
        }
    }
}";
            var fixedCode = @"
class TestClass
{
    public void TestMethod()
    {
        for (int i = 0; i < 10; i++)
        {
            var temp = i;
        }
    }
}";

            var expected = CSharpDiagnostic().WithLocation(9, 13);

            await VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task TestEmptyStatementInForStatementAsync()
        {
            var testCode = @"
class TestClass
{
    public void TestMethod()
    {
        for (;;)
        {
        }
    }
}";

            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task TestEmptyStatementAsync()
        {
            var testCode = @"
class TestClass
{
    public void TestMethod()
    {
        ;
    }
}";
            var fixedCode = @"
class TestClass
{
    public void TestMethod()
    {
    }
}";

            var expected = CSharpDiagnostic().WithLocation(6, 9);

            await VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task TestLabeledEmptyStatementAsync()
        {
            var testCode = @"
class TestClass
{
    public void TestMethod()
    {
    label:
        ;
    }
}";

            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task TestLabeledEmptyStatementFollowedByEmptyStatementAsync()
        {
            var testCode = @"
class TestClass
{
    public void TestMethod()
    {
    label:
        ;
        ;
    }
}";
            var fixedCode = @"
class TestClass
{
    public void TestMethod()
    {
    label:
        ;
    }
}";

            var expected = CSharpDiagnostic().WithLocation(8, 9);

            await VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task TestLabeledEmptyStatementFollowedByNonEmptyStatementAsync()
        {
            var testCode = @"
class TestClass
{
    public void TestMethod()
    {
    label:
        ;
        int x = 3;
    }
}";
            var fixedCode = @"
class TestClass
{
    public void TestMethod()
    {
    label:
        int x = 3;
    }
}";

            var expected = CSharpDiagnostic().WithLocation(7, 9);

            await VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task TestConsecutiveLabelsAsync()
        {
            var testCode = @"
class TestClass
{
    public void TestMethod()
    {
    label1:
    label2:
        ;
        int x = 3;
    }
}";
            var fixedCode = @"
class TestClass
{
    public void TestMethod()
    {
    label1:
    label2:
        int x = 3;
    }
}";

            var expected = CSharpDiagnostic().WithLocation(8, 9);

            await VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task TestSwitchCasesAsync()
        {
            var testCode = @"
class TestClass
{
    public void TestMethod()
    {
        switch (default(int))
        {
        case 0:
            ;
            break;

        case 1:
        case 2:
            ;
            break;

        default:
            ;
            break;
        }
    }
}";
            var fixedCode = @"
class TestClass
{
    public void TestMethod()
    {
        switch (default(int))
        {
        case 0:
            break;

        case 1:
        case 2:
            break;

        default:
            break;
        }
    }
}";

            DiagnosticResult[] expected =
            {
                CSharpDiagnostic().WithLocation(9, 13),
                CSharpDiagnostic().WithLocation(14, 13),
                CSharpDiagnostic().WithLocation(18, 13)
            };

            await VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        [TestCase("class Foo { }")]
        [TestCase("struct Foo { }")]
        [TestCase("interface IFoo { }")]
        [TestCase("enum Foo { }")]
        [TestCase("namespace Foo { }")]
        public async Task TestMemberAsync(string declaration)
        {
            var testCode = declaration + ";";
            var fixedCode = declaration;

            var expected = CSharpDiagnostic().WithLocation(1, declaration.Length + 1);

            await VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        /// <summary>
        /// Verifies that the code fix will remove all unnecessary whitespace.
        /// This is a regression for #1556
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task VerifyCodeFixWillRemoveUnnecessaryWhitespaceAsync()
        {
            var testCode = @"
class TestClass
{
    public void TestMethod1()
    {
        throw new System.NotImplementedException(); ;
    }

    public void TestMethod2()
    {
        throw new System.NotImplementedException(); /* c1 */ ;
    }
}";
            var fixedCode = @"
class TestClass
{
    public void TestMethod1()
    {
        throw new System.NotImplementedException();
    }

    public void TestMethod2()
    {
        throw new System.NotImplementedException(); /* c1 */
    }
}";

            DiagnosticResult[] expected =
            {
                CSharpDiagnostic().WithLocation(6, 53),
                CSharpDiagnostic().WithLocation(11, 62)
            };

            await VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        /// <summary>
        /// Verifies that the code fix will not remove relevant trivia.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task VerifyCodeFixWillNotRemoveTriviaAsync()
        {
            var testCode = @"
class TestClass
{
    public void TestMethod()
    {
        /* do nothing */ ;
    }
}";
            var fixedCode = @"
class TestClass
{
    public void TestMethod()
    {
        /* do nothing */
    }
}";

            var expected = CSharpDiagnostic().WithLocation(6, 26);

            await VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new SA1106CodeMustNotContainEmptyStatements();
        }

        /// <inheritdoc/>
        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new SA1106CodeFixProvider();
        }
    }
}