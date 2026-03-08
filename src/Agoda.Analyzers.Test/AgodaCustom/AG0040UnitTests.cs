using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using Microsoft.Playwright;

namespace Agoda.Analyzers.Test.AgodaCustom;

[TestFixture]
class AG0040UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0040WaitUntilStateNetworkIdleMustNotBeUsed();
        
    protected override string DiagnosticId => AG0040WaitUntilStateNetworkIdleMustNotBeUsed.DIAGNOSTIC_ID;
	    
    [Test]
    public async Task AG0040_WaitUntilStateNetworkIdle_ShouldShowWarning()
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

    [Test]
    public async Task AG0040_LoadStateNetworkIdle_ShouldShowWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] {typeof(LoadState).Assembly},
            Code = @"
				    using Microsoft.Playwright;
                    
                    class MyClass {
                        public MyClass () {
                           var a = LoadState.NetworkIdle;
                        }
                    }
                "
        };
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(6, 46));
    }

    [Test]
    public async Task AG0040_LoadStateDomContentLoaded_ShouldNotShowWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] {typeof(LoadState).Assembly},
            Code = @"
				    using Microsoft.Playwright;
                    
                    class MyClass {
                        public MyClass () {
                           var a = LoadState.DOMContentLoaded;
                        }
                    }
                "
        };
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0040_LoadStateLoad_ShouldNotShowWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] {typeof(LoadState).Assembly},
            Code = @"
				    using Microsoft.Playwright;
                    
                    class MyClass {
                        public MyClass () {
                           var a = LoadState.Load;
                        }
                    }
                "
        };
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }
}