using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.CodeFixes.AgodaCustom;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using AsyncAnalyzer;
using NUnit.Framework;

namespace AsyncAnalyzer.Test;

using VerifyCF = CSharpCodeFixVerifier<AG0039DetectLegacyAspNetBlockingCalls, AG0039FixLegacyAspNetBlockingCalls, NUnitVerifier>;

[TestFixture]
public class AG0039UnitTests
{
    [Test]
    public async Task WhenUsingDotResultInMethod_ShouldWarnUserOfRisks()
    {
        var test = @"
using System.Threading.Tasks;

internal static class Class1
{
	public static int Wrong()
	{
		return [|Delay1()|].Result;
	}
    async static Task<int> Delay1() { await Task.Delay (1000); return 1; }
}
                ";

        await VerifyCF.VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task WhenUsingDotResultInMethod_AndSingleBlockMethod_ShouldFixToAsyncAwait()
    {
        var test = @"
using System.Threading.Tasks;

internal static class Class1
{
	public static int Wrong()
	{
		return [|Delay1()|].Result;
	}
    async static Task<int> Delay1() { await Task.Delay (1000); return 1; }
}
                ";
        var expected = @"
using System.Threading.Tasks;

internal static class Class1
{
	public static async Task<int> Wrong()
	{
		return Delay1();
	}
    async static Task<int> Delay1() { await Task.Delay (1000); return 1; }
}
                ";
        await VerifyCF.VerifyCodeFixAsync(test, expected);
    }
}