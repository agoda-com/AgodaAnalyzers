using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom;

class AG0039UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0039UndocumentedMemberAnalyzer();

    protected override string DiagnosticId => AG0039UndocumentedMemberAnalyzer.DIAGNOSTIC_ID;

    [Test]
    public async Task AG0037_WithRegion_ShowsWarning()
    {
        var code = @"
				public class NotDoc
				{
					public string str1 {get;}
					public const int int2 = 1;
                    public void DoesNothing() {}
                    public event SampleEventHandler SampleEvent;
                    public delegate void SampleEventHandler(object sender);
				}
			";
        
        await VerifyDiagnosticsAsync(code, new []{
            new DiagnosticLocation(2, 18),
            new DiagnosticLocation(4, 20),
            new DiagnosticLocation(4, 26),
            new DiagnosticLocation(5, 23),
            new DiagnosticLocation(6, 33),
            new DiagnosticLocation(7, 53),
            new DiagnosticLocation(8, 42),
        });
    }
}