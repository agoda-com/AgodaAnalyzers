using System.Threading.Tasks;
using System.Web.Mvc;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom;

using VerifyCS = CSharpAnalyzerVerifier<AG0001DependencyResolverMustNotBeUsed, NUnitVerifier>;

[TestFixture]
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
        //await VerifyCS.VerifyAnalyzerAsync(code.Code);
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(8, 57));
    }

}