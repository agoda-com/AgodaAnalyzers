using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom;

[TestFixture]
[Parallelizable(ParallelScope.All)]
class AG0038UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0038PreventUseOfRegionPreprocessorDirective();

    protected override string DiagnosticId => AG0038PreventUseOfRegionPreprocessorDirective.DIAGNOSTIC_ID;

    [Test]
    public async Task AG0037_WithRegion_ShowsWarning()
    {
        var code = @"
				namespace RegionsSuck
				{
					#region
					class Something {}
					#endregion
				}
			";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(4, 6));
    }
}