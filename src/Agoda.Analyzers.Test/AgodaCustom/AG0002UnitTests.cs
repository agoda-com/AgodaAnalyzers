using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    class AG0002UnitTests : DiagnosticVerifier
    {
	    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0002PrivateMethodsShouldNotBeTested();
        
	    protected override string DiagnosticId => AG0002PrivateMethodsShouldNotBeTested.DIAGNOSTIC_ID;
	    
        [Test]
        public async Task TestCorrectDeclarationShouldNotCauseAnyIssue()
        {
            var code = @"
				interface ISomething {
					void DoSomething();
				}
			
				class TestClass : ISomething {
					public void DoSomething() {
					}
				}
			";

            await VerifyDiagnosticsAsync(code);
        }

        [Test]
        public async Task TestExtraPublicDeclarationShouldNotCauseAnyIssue()
        {
            var code = $@"
				interface ISomething {{
					void DoSomething();
				}}
			
				class TestClass : ISomething {{
					public void DoSomething() {{
					}}
                    public void DoSomething2() {{
					}}
				}}
			";

            await VerifyDiagnosticsAsync(code);
        }


        [Test]
        public async Task TestExplicitInterfaceImplementationShouldNotCauseAnyError()
        {
            var code = $@"
				interface ISomething {{
					void DoSomething();
				}}
			
				class TestClass : ISomething {{
					void ISomething.DoSomething() {{
					}}
				}}
			";

	        await VerifyDiagnosticsAsync(code);
        }

        [Test]
        public async Task TestMethodThatNotPartOfTheInterfaceShouldNotBeInternal()
        {
            var code = $@"
				interface ISomething {{
					void DoSomething();
				}}
			
				class TestClass : ISomething {{
					public void DoSomething() {{

					}}
                    internal void DoSomething2() {{

					}}
				}}
			";

            await VerifyDiagnosticsAsync(code , new DiagnosticLocation(10, 21));
        }
    }
}