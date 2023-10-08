using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Microsoft;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.CodeFixes.AgodaCustom
{
    public class AG0040ChangeElseIfToCase
    {
        private class CodeFixTest : CSharpCodeFixTest<AG0040TooManyElseIfConditions, AG0040TooManyElseIfConditionsFix, NUnitVerifier>
        {
            public CodeFixTest(
                string source,
                string fixedSource,
                params DiagnosticResult[] expected)
            {
                TestCode = source;
                FixedCode = fixedSource;
                ExpectedDiagnostics.AddRange(expected);

                ReferenceAssemblies = ReferenceAssemblies.Default
                    .AddPackages(ImmutableArray.Create(
                            new PackageIdentity("Shouldly", "4.2.1"),
                            new PackageIdentity("NUnit", "3.13.3")
                        )
                    );
            }
        }

        [Test]
        public async Task TestConversion()
        {
            var test = @"
    if (!mandatorySurcharge && !collectedByHotel)
    {
		cmsId = CmsId.RatePerNightOne;
	}
	else if (!mandatorySurcharge && collectedByHotel && dmcId == DmcId.YCS)
	{
		cmsId = CmsId.RatePerNightTwo;
	}
	else if (mandatorySurcharge && !collectedByHotel)
    {
        cmsId = CmsId.RatePerNightOne;
    }";

            var expected = @"
switch (true)
{
    case (!mandatorySurcharge && !collectedByHotel):
    case (mandatorySurcharge && !collectedByHotel):
        cmsId = CmsId.RatePerNightOne;
        break;

    case (!mandatorySurcharge && collectedByHotel && dmcId == DmcId.YCS):
        cmsId = CmsId.RatePerNightTwo;
        break;

    default:
        // Handle any other cases here
        break;
}";
            var codeFixTest = new CodeFixTest(test, expected,
                CSharpAnalyzerVerifier<AG0040TooManyElseIfConditions, NUnitVerifier>
                    .Diagnostic(AG0040TooManyElseIfConditions.DiagnosticId)
                    .WithSpan(13, 13, 13, 55));

            await codeFixTest.RunAsync(CancellationToken.None);
            var a = codeFixTest.CompilerDiagnostics;
        }

    }
}
