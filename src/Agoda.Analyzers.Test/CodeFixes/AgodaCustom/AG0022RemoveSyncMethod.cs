using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.CodeFixes.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.CodeFixes.AgodaCustom
{
    class AG0022RemoveSyncMethodUnitTests : CodeFixVerifier
    {
        [Test]
        public async Task AG0022_ShouldRemoveSyncVersion()
        {
            var code = @"
using System.Threading.Tasks;

interface TestInterface
{
    bool TestMethod(string url);
    Task<bool> TestMethodAsync(string url);
}";

            var result = @"
using System.Threading.Tasks;

interface TestInterface
{
    Task<bool> TestMethodAsync(string url);
}";
            
            var expected = CSharpDiagnostic(AG0022DoNotExposeBothSyncAndAsyncVersionsOfMethods.DiagnosticId);
            var diagnosticResults = new[]
            {
                expected.WithLocation(6, 5),
                expected.WithLocation(6, 5)
            };
            
            await RunTest(code, result, diagnosticResults);
        }
        
        [Test]
        public async Task AG0022_ShouldRemoveSyncVersionWithComment()
        {
            var code = @"
using System.Threading.Tasks;

interface TestInterface
{
    /// <summary>
    /// This is a summary for TestMethod
    /// </summary>
    bool TestMethod(string url); // comment

    /// <summary>
    /// This is a summary for TestMethod
    /// </summary>
    Task<bool> TestMethodAsync(string url); // comment
}";

            var result = @"
using System.Threading.Tasks;

interface TestInterface
{

    /// <summary>
    /// This is a summary for TestMethod
    /// </summary>
    Task<bool> TestMethodAsync(string url); // comment
}";
            
            var expected = CSharpDiagnostic(AG0022DoNotExposeBothSyncAndAsyncVersionsOfMethods.DiagnosticId);
            var diagnosticResults = new[]
            {
                expected.WithLocation(9, 5),
                expected.WithLocation(9, 5)
            };
            
            await RunTest(code, result, diagnosticResults);
        }
        
        private async Task RunTest(string code, string result, DiagnosticResult[] diagnosticResults)
        {
            var reference = MetadataReference.CreateFromFile(typeof(Task).Assembly.Location);
            var doc = CreateProject(new[] {code})
                .AddMetadataReference(reference)
                .Documents
                .First();
            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();
            var diag = await GetSortedDiagnosticsFromDocumentsAsync(
                analyzersArray,
                new[] {doc},
                CancellationToken.None
            ).ConfigureAwait(false);            
            VerifyDiagnosticResults(
                diag,
                analyzersArray,
                diagnosticResults);
            await VerifyCSharpFixAsync(code, result);
        }
        
        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new AG0022RemoveSyncMethod();
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0022DoNotExposeBothSyncAndAsyncVersionsOfMethods();
        }
    }
}
