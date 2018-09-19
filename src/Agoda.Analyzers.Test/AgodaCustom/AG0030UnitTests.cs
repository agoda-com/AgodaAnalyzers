using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    internal class AG0030UnitTests : DiagnosticVerifier
    {
	    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0030PreventUseOfDynamics();
	    
	    protected override string DiagnosticId => AG0030PreventUseOfDynamics.DIAGNOSTIC_ID;
        
        [Test]
        public async Task AG0030_WhenNoDynamic_ShouldntShowAnyWarning()
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

	        await VerifyDiagnosticResults(code);
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
            await VerifyDiagnosticResults(code, expected);
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
            await VerifyDiagnosticResults(code, expected);
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
            await VerifyDiagnosticResults(code, expected);
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

            await VerifyDiagnosticResults(code);
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
            await VerifyDiagnosticResults(code, expected);
        }


	    
    }
}