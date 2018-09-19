using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    internal class AG0022UnitTests : DiagnosticVerifier
    {
        [Test]
        public async Task AG0022_WhenNotExistBothSyncAndAsyncVersionsOnInterface_ShouldNotShowWarning()
        {
            const string code = @"
using System.Threading.Tasks;

interface TestInterface
{
    bool TestMethod(string url);
    Task<bool> NotTestMethodAsync(string url);
}
";

            await TestForResults(code);
        }
        
        [Test]
        public async Task AG0022_WhenNotExistBothSyncAndAsyncVersionsOnClass_ShouldNotShowWarning()
        {
            const string code = @"
using System.Threading.Tasks;

class TestClass
{
    public bool TestMethod(string url)
    {
        return true;
    }
    public Task<bool> NotTestMethodAsync(string url) 
    { 
        return Task.FromResult(true);
    }
}
";

            await TestForResults(code);
        }
        
        [Test]
        public async Task AG0022_WhenExistBothSyncAndAsyncVersionsOfMethodsOnInterface_ShouldShowWarning()
        {
            const string code = @"
using System.Threading.Tasks;

interface Interface
{
    bool TestMethod(string url);
    Task<bool> TestMethodAsync(string url);
}
			";

            var baseResult = CSharpDiagnostic(AG0022DoNotExposeBothSyncAndAsyncVersionsOfMethods.DIAGNOSTIC_ID);
            var expected = new []
            {
                baseResult.WithLocation(6, 5)
            };

            await TestForResults(code, expected);
        }

        [Test]
        public async Task AG0022_WhenExistBothSyncAndAsyncVersionsOnClass_ShouldShowWarning()
        {
            const string code = @"
using System.Threading.Tasks;

class TestClass
{
    public bool TestMethod(string url)
    {
        return true;
    }
    public Task<bool> TestMethodAsync(string url) 
    { 
        return Task.FromResult(true); 
    }
}
";

            var baseResult = CSharpDiagnostic(AG0022DoNotExposeBothSyncAndAsyncVersionsOfMethods.DIAGNOSTIC_ID);
            var expected = new[]
            {
                baseResult.WithLocation(6, 5),
            };

            await TestForResults(code, expected);
        }
        
        [Test]
        public async Task AG0022_WhenExistBothSyncAndAsyncVoidVersions_ShouldShowWarning()
        {
            const string code = @"
using System.Threading.Tasks;

class TestClass
{
    public void TestMethod(string url)
    {
        return;
    }
    public async void TestMethodAsync(string url) 
    { 
        
    }
}
";

            var baseResult = CSharpDiagnostic(AG0022DoNotExposeBothSyncAndAsyncVersionsOfMethods.DIAGNOSTIC_ID);
            var expected = new[]
            {
                baseResult.WithLocation(6, 5),
            };

            await TestForResults(code, expected);
        }
        
        [Test]
        public async Task AG0022_WhenExistBothSyncAndAsyncMethodsNamedSame_ShouldShowWarning()
        {
            const string code = @"
using System.Threading.Tasks;

class TestClass
{
    public bool TestMethod(string url)
    {
        return true;
    }
    public Task<bool> TestMethod(int something) 
    { 
        return Task.FromResult(true); 
    }
}
";

            var baseResult = CSharpDiagnostic(AG0022DoNotExposeBothSyncAndAsyncVersionsOfMethods.DIAGNOSTIC_ID);
            var expected = new[]
            {
                baseResult.WithLocation(6, 5),
            };

            await TestForResults(code, expected);
        }
        
        [Test]
        public async Task AG0022_WhenExistBothSyncAndAsyncVersionsOfInternalMethods_ShouldNotShowWarning()
        {
            const string code = @"
using System.Threading.Tasks;

class TestClass
{
    internal bool TestMethod(string url)
    {
        return true;
    }
    internal Task<bool> TestMethodAsync(string url) 
    { 
        return Task.FromResult(true); 
    }
}
";
            await TestForResults(code);
        }

        private async Task TestForResults(string code, DiagnosticResult[] expected = null)
        {
            expected = expected ?? new DiagnosticResult[0];
            var reference = MetadataReference.CreateFromFile(typeof(Task).Assembly.Location);

            var doc = CreateProject(new[] {code})
                .AddMetadataReference(reference)
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diagnostics = 
                await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] {doc}, CancellationToken.None)
                     .ConfigureAwait(false);

            VerifyDiagnosticResults(diagnostics, analyzersArray, expected);
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0022DoNotExposeBothSyncAndAsyncVersionsOfMethods();
        }
    }
}
