using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom;

[TestFixture]
class AG0039UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0039MethodLineLengthAnalyzer();

    protected override string DiagnosticId => AG0039MethodLineLengthAnalyzer.DIAGNOSTIC_ID;
    
    [Test]
    public async Task AG0039_At40LinesOfCode_ShowWarning()
    {
        var code = @"
                internal class InternalClass
                {
                    private void MyMethod()
                    {
var a = 1;
";
        for (int i = 0; i <= 121; i++)
        {
            code += "a += 1;\n";
        }
        code += @"
                    }
                }
";
        await VerifyDiagnosticsAsync(code, new[]{
            new DiagnosticLocation(4, 34, "MyMethod","126", "40"),
        });
    }

    [Test]
    public async Task AG0039_At40LinesOfWhiteSpace_DoNotShowWarning()
    {
        var code = @"
                internal class InternalClass
                {
                    private void MyMethod()
                    {
var a = 1;
";
        for (int i = 0; i <= 121; i++)
        {
            code += "\n";
        }
        code += @"
                    }
                }
";
        await VerifyDiagnosticsAsync(code,EmptyDiagnosticResults);
    }
}
