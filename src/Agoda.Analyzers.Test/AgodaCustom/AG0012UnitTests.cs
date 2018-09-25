using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    internal class AG0012UnitTests : DiagnosticVerifier
    {
        protected DiagnosticAnalyzer DiagnosticAnalyzer => new AG0012TestMethodMustContainAtLeastOneAssertion();
        
        protected string DiagnosticId => AG0012TestMethodMustContainAtLeastOneAssertion.DIAGNOSTIC_ID;
        
        [Test]
        public async Task AG0012_WithNoAssertion_ShouldShowWarning()
        {
            var code = new CodeDescriptor
            {
                References = new[] {typeof(TestFixtureAttribute).Assembly},
                Code = @"
                    using NUnit.Framework;
                    using System;
                    
                    namespace Tests
                    {
                        public class TestClass
                        {
                            [Test]
                            public void This_Is_Not_Valid()
                            {
                                int[] arrayForShouldBe = { 1, 2, 3 };
                            }
                        }
                    }"
            };
 
            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(9, 29), DiagnosticId, DiagnosticAnalyzer);
        }
        
        [Test]
        public async Task AG0012_WithNUnitAssertion_ShouldNotShowWarning()
        {
            var code = new CodeDescriptor
            {
                References = new[] {typeof(TestFixtureAttribute).Assembly, typeof(Shouldly.Should).Assembly},
                Code = @"
                    using NUnit.Framework;
                    
                    namespace Tests
                    {
                        public class TestClass
                        {
                            [Test]
                            public void This_Is_Valid()
                            {
                                int[] arrayToAssert = { 1, 2, 3 };
                                Assert.That(arrayToAssert, Has.Exactly(1).EqualTo(3));
                            }
                        }
                    }"
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults, DiagnosticId, DiagnosticAnalyzer);
        }

 
        
        [Test]
        public async Task AG0012_WithShouldlyAssertion_ShouldNotShowWarning()
        {
            var code = new CodeDescriptor
            {
                References = new[] {typeof(TestFixtureAttribute).Assembly, typeof(Shouldly.Should).Assembly},
                Code = @"
                    using NUnit.Framework;
                    using Shouldly;
                    using System;
                    
                    namespace Tests
                    {
                        public class TestClass
                        {
                            [Test]
                            public void This_Is_Valid()
                            {
                                int[] arrayForShouldBe = { 1, 2, 3 };
                                arrayForShouldBe.Length.ShouldBe(3);
                            }
                    
                            [Test]
                            public void This_Is_AlsoValid()
                            {
                                Should.Throw<Exception>(() => {
                                    var y = 1;
                                });
                            }
                        }
                    }"
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults, DiagnosticId, DiagnosticAnalyzer);
        }
    }
}