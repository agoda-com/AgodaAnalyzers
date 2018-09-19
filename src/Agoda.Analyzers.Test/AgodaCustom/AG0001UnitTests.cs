using System.Threading.Tasks;
using System.Web.Mvc;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    class AG0001UnitTests : DiagnosticVerifier
    {
	    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0001DependencyResolverMustNotBeUsed();
        
	    protected override string DiagnosticId => AG0001DependencyResolverMustNotBeUsed.DIAGNOSTIC_ID;
	    
        [Test]
        public async Task TestDependencyResolverUsageAsync()
        {
            var code = @"
				interface ISomething {
					void DoSomething();
				}
			
				class TestClass {
					public void TestMethod() {
						var instance = System.Web.Mvc.DependencyResolver.Current.GetService(typeof(ISomething));
						//instance.DoSomething();
					}
				}
			";

            await VerifyDiagnosticsAsync(code, typeof(DependencyResolver).Assembly, new DiagnosticLocation(8, 37));
        }

    }
}