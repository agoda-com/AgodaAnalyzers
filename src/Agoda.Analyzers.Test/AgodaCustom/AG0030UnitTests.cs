using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    internal class AG0030UnitTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0030PreventUseOfDynamics();

        protected override string DiagnosticId => AG0030PreventUseOfDynamics.DIAGNOSTIC_ID;

        [Test]
        public async Task AG0030_WhenNoDynamic_ShouldntShowWarning()
        {
            var code = @"
				class TestClass {
					public void TestMethod1() {
						int instance = 1;
					}

                    public int TestMethod2() {
						return 1;
					}
				}
			";

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0030_WhenMethodReturnsDynamic_ShowWarning()
        {
            var code = @"
				class TestClass {
					public dynamic TestMethod2() {
						return 1;
					}
				}
			";

            var expected = new DiagnosticLocation(3, 6);
            await VerifyDiagnosticsAsync(code, expected);
        }

        [Test]
        public async Task AG0030_WhenDynamicVariableDeclared_ShowWarning()
        {
            var code = @"
				class TestClass {
					public void TestMethod1() {
						dynamic instance = 1;
					}
				}
			";

            var expected = new DiagnosticLocation(4, 7);
            await VerifyDiagnosticsAsync(code, expected);
        }

        [Test]
        public async Task AG0030_WhenMultipleDynamicUsed_ShowWarning()
        {
            var code = @"
				class TestClass {
					public dynamic TestMethod2() {
						return 1;
					}

					public void TestMethod1() {
						dynamic instance = 1;
					}
				}
			";

            var expected = new[]
            {
                new DiagnosticLocation(3, 6),
                new DiagnosticLocation(8, 7)
            };
            await VerifyDiagnosticsAsync(code, expected);
        }

        [Test]
        public async Task AG0030_WhenReturnTypeContainsTheStringDynamic_ShouldntShowAnyWarning()
        {
            var code = @"        
                class Xdynamic { }
 
                class TestClass {
                    public Xdynamic TestMethod2() {
                        return null;
                    }
                }
                ";

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0030_WhenDynamicInInterface_ShowWarning()
        {
            var code = @"
				interface TestInterface {
					dynamic TestMethod1();
				}
			";

            var expected = new DiagnosticLocation(3, 6);
            await VerifyDiagnosticsAsync(code, expected);
        }

        [Test]
        public async Task AG0030_WhenClassHasDynamicAsGenericParameter_ShouldShowWarning()
        {
            var code = @"
				class TestClass<T> {
					public void TestMethod1() {
						 var test = new TestClass<dynamic>();
					}
				}
			";

            var expected = new DiagnosticLocation(4, 23);
            await VerifyDiagnosticsAsync(code, expected);
        }

        [Test]
        public async Task AG0030_WhenClassHasDynamicAsOneOfManyGenericParameters_ShouldShowWarning()
        {
            var code = @"
				class TestClass<T1, T2, T3> {
					public void TestMethod1() {
						 var test = new TestClass<bool, dynamic, int>();
					}
				}
			";

            var expected = new DiagnosticLocation(4, 23);
            await VerifyDiagnosticsAsync(code, expected);
        }

        [Test]
        public async Task AG0030_WhenClassDoesNotHaveDynamicAsGenericParameter_ShouldNotShowWarning()
        {
            var code = @"
				class TestClass<T1, T2, T3> {
					public void TestMethod1() {
						 var test = new TestClass<bool, long, int>();
					}
				}
			";

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0030_WhenMethodHasDynamicAsGenericParameter_ShouldShowWarning()
        {
            var code = @"
				class TestClass {
                    public void GenericMethod<T>() { }
					
                    public void TestMethod1() {
						 GenericMethod<dynamic>();
					}
				}
			";

            var expected = new DiagnosticLocation(6, 8);
            await VerifyDiagnosticsAsync(code, expected);
        }

        [Test]
        public async Task AG0030_WhenMethodHasDynamicAsOneOfManyGenericParameters_ShouldShowWarning()
        {
            var code = @"
				class TestClass {
                    public void GenericMethod<T1, T2, T3, T4>() { }
					
                    public void TestMethod1() {
						 GenericMethod<bool, long, dynamic, int>();
					}
				}
			";

            var expected = new DiagnosticLocation(6, 8);
            await VerifyDiagnosticsAsync(code, expected);
        }

        [Test]
        public async Task AG0030_WhenMethodDoesNotHaveDynamicAsGenericParameter_ShouldNotShowWarning()
        {
            var code = @"
				class TestClass {
                    public void GenericMethod<T1, T2, T3, T4>() { }
					
                    public void TestMethod1() {
						 GenericMethod<bool, long, string, int>();
					}
				}
			";

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }
    }
}
