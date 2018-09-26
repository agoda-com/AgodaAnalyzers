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
        protected DiagnosticAnalyzer DiagnosticAnalyzer => new SA1106CodeMustNotContainEmptyStatements();
        
        protected string DiagnosticId => SA1106CodeMustNotContainEmptyStatements.DIAGNOSTIC_ID;
        
        protected CodeFixProvider CodeFixProvider => new SA1106CodeFixProvider();
        
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

            await VerifyDiagnosticsAsync(testCode, new DiagnosticLocation(7, 13), DiagnosticId, DiagnosticAnalyzer);
            await VerifyDiagnosticsAsync(fixedCode, EmptyDiagnosticResults, DiagnosticId, DiagnosticAnalyzer);
            await VerifyCodeFixAsync(testCode, fixedCode, CodeFixProvider, DiagnosticAnalyzer);
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
}
";
            
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
}
";

            await VerifyDiagnosticsAsync(testCode, new DiagnosticLocation(7, 13), DiagnosticId, DiagnosticAnalyzer);
            await VerifyDiagnosticsAsync(fixedCode, EmptyDiagnosticResults, DiagnosticId, DiagnosticAnalyzer);
            await VerifyCodeFixAsync(testCode, fixedCode, CodeFixProvider, DiagnosticAnalyzer);
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
                }
            ";
            
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
                }
            ";

            await VerifyDiagnosticsAsync(testCode, new DiagnosticLocation(9, 29), DiagnosticId, DiagnosticAnalyzer);
            await VerifyDiagnosticsAsync(fixedCode, EmptyDiagnosticResults, DiagnosticId, DiagnosticAnalyzer);
            await VerifyCodeFixAsync(testCode, fixedCode, CodeFixProvider, DiagnosticAnalyzer);
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
                }
            ";

            await VerifyDiagnosticsAsync(testCode, EmptyDiagnosticResults, DiagnosticId, DiagnosticAnalyzer);
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
                }
            ";
            
            var fixedCode = @"
                class TestClass
                {
                    public void TestMethod()
                    {
                    }
                }
            ";

            await VerifyDiagnosticsAsync(testCode, new DiagnosticLocation(6, 25), DiagnosticId, DiagnosticAnalyzer);
            await VerifyDiagnosticsAsync(fixedCode, EmptyDiagnosticResults, DiagnosticId, DiagnosticAnalyzer);
            await VerifyCodeFixAsync(testCode, fixedCode, CodeFixProvider, DiagnosticAnalyzer);
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
                }
            ";

            await VerifyDiagnosticsAsync(testCode, EmptyDiagnosticResults, DiagnosticId, DiagnosticAnalyzer);
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
                }
            ";
            
            var fixedCode = @"
                class TestClass
                {
                    public void TestMethod()
                    {
                    label:
                        ;
                    }
                }
            ";

            await VerifyDiagnosticsAsync(testCode, new DiagnosticLocation(8, 25), DiagnosticId, DiagnosticAnalyzer);
            await VerifyDiagnosticsAsync(fixedCode, EmptyDiagnosticResults, DiagnosticId, DiagnosticAnalyzer);
            await VerifyCodeFixAsync(testCode, fixedCode, CodeFixProvider, DiagnosticAnalyzer);
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
                }
            ";
            
            var fixedCode = @"
                class TestClass
                {
                    public void TestMethod()
                    {
                    label:
                        int x = 3;
                    }
                }
            ";

            await VerifyDiagnosticsAsync(testCode, new DiagnosticLocation(7, 25), DiagnosticId, DiagnosticAnalyzer);
            await VerifyDiagnosticsAsync(fixedCode, EmptyDiagnosticResults, DiagnosticId, DiagnosticAnalyzer);
            await VerifyCodeFixAsync(testCode, fixedCode, CodeFixProvider, DiagnosticAnalyzer);
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
                }
            ";
            
            var fixedCode = @"
                class TestClass
                {
                    public void TestMethod()
                    {
                    label1:
                    label2:
                        int x = 3;
                    }
                }
            ";

            await VerifyDiagnosticsAsync(testCode, new DiagnosticLocation(8, 25), DiagnosticId, DiagnosticAnalyzer);
            await VerifyDiagnosticsAsync(fixedCode, EmptyDiagnosticResults, DiagnosticId, DiagnosticAnalyzer);
            await VerifyCodeFixAsync(testCode, fixedCode, CodeFixProvider, DiagnosticAnalyzer);
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
                }
            ";
            
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
                }
            ";

            var expected = new []
            {
                new DiagnosticLocation(9, 29),
                new DiagnosticLocation(14, 29),
                new DiagnosticLocation(18, 29)
            };

            await VerifyDiagnosticsAsync(testCode, expected, DiagnosticId, DiagnosticAnalyzer);
            await VerifyDiagnosticsAsync(fixedCode, EmptyDiagnosticResults, DiagnosticId, DiagnosticAnalyzer);
            await VerifyCodeFixAsync(testCode, fixedCode, CodeFixProvider, DiagnosticAnalyzer);
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

            var expected = new DiagnosticLocation(1, declaration.Length + 1);

            await VerifyDiagnosticsAsync(testCode, expected, DiagnosticId, DiagnosticAnalyzer);
            await VerifyDiagnosticsAsync(fixedCode, EmptyDiagnosticResults, DiagnosticId, DiagnosticAnalyzer);
            await VerifyCodeFixAsync(testCode, fixedCode, CodeFixProvider, DiagnosticAnalyzer);
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
                }
            ";
            
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
                }
            ";

            var expected = new []
            {
                new DiagnosticLocation(6, 69),
                new DiagnosticLocation(11, 78)
            };

            await VerifyDiagnosticsAsync(testCode, expected, DiagnosticId, DiagnosticAnalyzer);
            await VerifyDiagnosticsAsync(fixedCode, EmptyDiagnosticResults, DiagnosticId, DiagnosticAnalyzer);
            await VerifyCodeFixAsync(testCode, fixedCode, CodeFixProvider, DiagnosticAnalyzer);
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
                }
            ";
            
            var fixedCode = @"
                class TestClass
                {
                    public void TestMethod()
                    {
                        /* do nothing */
                    }
                }
            ";

            var expected = new DiagnosticLocation(6, 42);

            await VerifyDiagnosticsAsync(testCode, expected, DiagnosticId, DiagnosticAnalyzer);
            await VerifyDiagnosticsAsync(fixedCode, EmptyDiagnosticResults, DiagnosticId, DiagnosticAnalyzer);
            await VerifyCodeFixAsync(testCode, fixedCode, CodeFixProvider, DiagnosticAnalyzer);
        }
    }
}