using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    internal class AG0023UnitTests : DiagnosticVerifier
    {
       
        [Test]
        public async Task AG0023_WhenThreadYield_ShouldNotShowWarning()
        {
            var code = @"
using System.Threading;

class TestClass 
{
    public void TestMethod1() 
    {
        Thread.Yield();
    }
}
";

            await Assert(code);
        }

        [Test]
        public async Task AG0023_WhenDifferentNameSpaceThreadSleep_ShouldNotShowWarning()
        {
            var code = @"
public static class Thread {
    public static void Sleep(int time) {}
}

class TestClass 
{
    
    public void TestMethod1() 
    {
        Thread.Sleep(100);
    }
}
";
            await Assert(code);
        }

        [Test]
        public async Task AG0023_WhenThreadSleepWithInt_ShouldShowWarning()
        {
            var code = @"
using System.Threading;

class TestClass 
{
    public void TestMethod1() 
    {
        Thread.Sleep(100);
    }
}
";

            var baseResult = CSharpDiagnostic(AG0023PreventUseOfThreadSleep.DIAGNOSTIC_ID);
            var expected = new[]
            {
                baseResult.WithLocation(8, 9)
            };

            await Assert(code, expected);
        }

        [Test]
        public async Task AG0023_WhenThreadSleepWithTimeSpan_ShouldShowWarning()
        {
            var code = @"
using System.Threading;
using System;

class TestClass 
{
    public void TestMethod1() 
    {
        Thread.Sleep(TimeSpan.FromMilliseconds(3000));
    }
}
";
            var baseResult = CSharpDiagnostic(AG0023PreventUseOfThreadSleep.DIAGNOSTIC_ID);
            var expected = new[]
            {
                baseResult.WithLocation(9, 9)
            };

            await Assert(code, expected);
        }

        private async Task Assert(string code, DiagnosticResult[] expected = null)
        {
            expected = expected ?? new DiagnosticResult[0];
            var doc = CreateProject(new[] { code })
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] { doc }, CancellationToken.None)
                .ConfigureAwait(false);


            VerifyDiagnosticResults(diag, analyzersArray, expected);
        }


        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0023PreventUseOfThreadSleep();
        }
    }

}
