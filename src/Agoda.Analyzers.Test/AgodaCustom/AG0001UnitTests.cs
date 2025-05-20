using System.Threading.Tasks;
using System.Web.Mvc;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom;

[TestFixture]
[Parallelizable(ParallelScope.All)]
class AG0001UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0001DependencyResolverMustNotBeUsed();
        
    protected override string DiagnosticId => AG0001DependencyResolverMustNotBeUsed.DIAGNOSTIC_ID;
	    
    [Test]
    public async Task TestDependencyResolverUsageAsync()
    {
        var code = new CodeDescriptor
        {
            References = new[] {typeof(DependencyResolver).Assembly},
            Code = @"
					interface ISomething {
						void DoSomething();
					}
				
					class TestClass {
						public void TestMethod() {
							var instance = System.Web.Mvc.DependencyResolver.Current.GetService(typeof(ISomething));
						}
					}"
        };
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(8, 57));
    }

}