using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom;

[TestFixture]
[Parallelizable(ParallelScope.All)]
class AG0004UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0004DoNotUseHardCodedStringsToIdentifyTypes();

    protected override string DiagnosticId => AG0004DoNotUseHardCodedStringsToIdentifyTypes.DIAGNOSTIC_ID;

    [Test]
    public async Task AG0004_ForTypeGetType_ShowsWarning()
    {
        var code = @"
				using System;

				class TestClass
				{
					public void Test()
					{
						var type = Type.GetType(""System.String"");
					}
				}";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(8, 18));
    }
		
    [Test]
    public async Task AG0004_ForActivatorCreateInstanceWithStringArg_ShowsWarning()
    {
        var code = @"
				using System;

				class TestClass
				{
					public void Test()
					{
						var instance = Activator.CreateInstance(""System"", ""System.String"");
					}
				}";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(8, 22));
    }
		
    [Test]
    public async Task AG0004_ForActivatorCreateInstanceWithTypeArg_DoesNotShowWarning()
    {
        var code = @"
				using System;

				class TestClass
				{
					public void Test()
					{
						var instance = Activator.CreateInstance(typeof(System.String));
					}
				}";

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }
}