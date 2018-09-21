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
        public async Task AG0013_WithLessThan5Inputs_ShouldNotShowWarning()
        {
            var code = new CodeDescriptor
            {
                References = new[] {typeof(TestFixtureAttribute).Assembly},
                Code = @"
                    using NUnit.Framework;

                    namespace Test
                    {
                        public class TestClass
                        {
                            [Test]
                            public void This_Is_Valid(int a1, int a2, int a3, int a4, int a5) { }
                        }
                    }"
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }
        
        [Test]
        public async Task AG0013_With5OrMoreInputs_ShouldShowWarning()
        {
            var code = new CodeDescriptor
            {
                References = new[] {typeof(TestFixtureAttribute).Assembly},
                Code = @"
                    using NUnit.Framework;
        
                    namespace Test
                    {
                        public class TestClass
                        {
                            [Test]
                            public void This_Is_NotValid(int a1, int a2, int a3, int a4, int a5, int a6) { }
                        }
                    }"
            };

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(8, 29));
        }
    }
}
