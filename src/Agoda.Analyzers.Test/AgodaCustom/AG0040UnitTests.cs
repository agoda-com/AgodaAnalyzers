using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using Microsoft.Playwright;

namespace Agoda.Analyzers.Test.AgodaCustom;

[TestFixture]
[Parallelizable(ParallelScope.All)]
class AG0040UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0040WaitUntilStateNetworkIdleMustNotBeUsed();
        
    protected override string DiagnosticId => AG0040WaitUntilStateNetworkIdleMustNotBeUsed.DIAGNOSTIC_ID;
	    
    [Test]
    public async Task TestDependencyResolverUsageAsync()
    {
        var code = new CodeDescriptor
        {
            References = new[] {typeof(WaitUntilState).Assembly},
            Code = @"
				    using Microsoft.Playwright;
                    
                    class MyClass {
                        public MyClass () {
                           var a = WaitUntilState.NetworkIdle;
                        }
                    }
                "
        };
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(6, 51));
    }

}