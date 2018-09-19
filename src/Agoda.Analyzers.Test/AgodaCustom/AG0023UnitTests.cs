using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    internal class AG0023UnitTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0023PreventUseOfThreadSleep();
        
        protected override string DiagnosticId => AG0023PreventUseOfThreadSleep.DIAGNOSTIC_ID;
       
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

            await VerifyDiagnosticResults(code);
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
            await VerifyDiagnosticResults(code);
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

            await VerifyDiagnosticResults(code, new DiagnosticLocation(8, 9));
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

            await VerifyDiagnosticResults(code, new DiagnosticLocation(9, 9));
        }
    }

}
