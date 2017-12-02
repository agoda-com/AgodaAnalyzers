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
    /// <summary>
    /// This class contains unit tests for <see cref="SA1123DoNotPlaceRegionsWithinElements"/> and
    /// <see cref="RemoveRegionCodeFixProvider"/>.
    /// </summary>
    public class SA1123UnitTests : CodeFixVerifier
    {
        [Test]
        public async Task TestRegionInMethodAsync()
        {
            var testCode = @"public class Foo
{
    public void Bar()
    {
#region Foo
        string test = """";
#endregion
    }
}";

            var expected = CSharpDiagnostic().WithLocation(5, 1);

            await VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);

            var fixedCode = @"public class Foo
{
    public void Bar()
    {
        string test = """";
    }
}";

            await VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
            await VerifyCSharpFixAllFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task TestRegionPartialyInMethodAsync()
        {
            var testCode = @"public class Foo
{
    public void Bar()
    {
#region Foo
        string test = """";
    }
#endregion
}";

            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task TestRegionPartialyInMethod2Async()
        {
            var testCode = @"public class Foo
{
    public void Bar()
#region Foo
    {
        string test = """";
    }
#endregion
}";
            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task TestRegionPartialyMultipleMethodsAsync()
        {
            var testCode = @"public class Foo
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
}";
            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task TestEndRegionInMethodAsync()
        {
            var testCode = @"public class Foo
{
#region Foo
    public void Bar()
    {
        string test = """";
#endregion
    }
}";

            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task TestRegionOutsideMethodAsync()
        {
            var testCode = @"public class Foo
{
#region Foo
#endregion
    public void Bar()
    {
        string test = """";
    }
}";

            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task TestRegionOutsideMethod2Async()
        {
            var testCode = @"public class Foo
{
#region Foo
    public void Bar()
    {
        string test = """";
    }
#endregion
}";

            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
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
            await VerifyCSharpFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpFixAllFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new SA1123DoNotPlaceRegionsWithinElements();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new RemoveRegionCodeFixProvider();
        }
    }
}