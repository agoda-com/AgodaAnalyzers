using NUnit.Framework;
using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    class AG0013UnitTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0013LimitNumberOfTestMethodParametersTo5();
        
        protected override string DiagnosticId => AG0013LimitNumberOfTestMethodParametersTo5.DIAGNOSTIC_ID;

        [Test]
        public async Task TestMethod_ShouldNotTake_MoreThan5Inputs()
        {
            var testCode = @"
            using NUnit.Framework;

            namespace Test
            {
                [TestFixture]
                public class Test_Method
                {
                    [Test]
                    [TestCase(0, true)]
                    public void This_Is_Valid(int a, bool expected) { }

                    [Test]
                    [TestCase(0, 1, 2, 3, 4, true)]
                    public void This_Is_NotValid(int a, int b, int c, int d, int e, bool expected) { }
                }
            }";

            await VerifyDiagnosticsAsync(testCode, typeof(TestFixtureAttribute).Assembly, new DiagnosticLocation(13, 21));
        }
    }
}
