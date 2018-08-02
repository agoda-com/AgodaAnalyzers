using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    public class AG0004UnitTests : DiagnosticVerifier
    {
        [Test]
        public async Task Wait_Should_Trigger_Validation()
        {
            var code = $@"
                    using System.Threading;
                    using System.Threading.Tasks;
                    
                    public class TestClass
                    {{
	                    public void Test()
	                    {{
		                     {{
			                    var a = new Task(() => {{}});
			                    a.Wait();
		                    }}
	                  }}
                   }}

            ";

            var nUnit = MetadataReference.CreateFromFile(typeof(TestFixtureAttribute).Assembly.Location);

            var doc = CreateProject(new[] { code })
                .AddMetadataReference(nUnit)
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] { doc }, CancellationToken.None).ConfigureAwait(false);

            var baseResult = CSharpDiagnostic(AG0004AsyncAnalyzer.DiagnosticId);
            VerifyDiagnosticResults(diag, analyzersArray, new[]
            {
                baseResult.WithLocation(11, 26),
            });
        }

        [Test]
        public async Task WaitAll_Should_Trigger_Validation()
        {
            var code = $@"
                    using System.Threading;
                    using System.Threading.Tasks;
                    
                    public class TestClass
                    {{
	                    public void Test()
	                    {{
		                     {{
			                    var a = new Task(() => {{}});
			                    Task.WaitAll(a);
		                    }}
	                  }}
                   }}

            ";

            var nUnit = MetadataReference.CreateFromFile(typeof(TestFixtureAttribute).Assembly.Location);
          
            var doc = CreateProject(new[] { code })
                .AddMetadataReference(nUnit)
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] { doc }, CancellationToken.None).ConfigureAwait(false);

            var baseResult = CSharpDiagnostic(AG0004AsyncAnalyzer.DiagnosticId);
            VerifyDiagnosticResults(diag, analyzersArray, new[]
            {
                baseResult.WithLocation(11, 29),
            });
        }

        [Test]
        public async Task WaitAny_Should_Trigger_Validation()
        {
            var code = $@"
                    using System.Threading;
                    using System.Threading.Tasks;
                    
                    public class TestClass
                    {{
	                    public void Test()
	                    {{
		                     {{
			                    var a = new Task(() => {{}});
			                    Task.WaitAny(a);
		                    }}
	                  }}
                   }}

            ";

            var nUnit = MetadataReference.CreateFromFile(typeof(TestFixtureAttribute).Assembly.Location);

            var doc = CreateProject(new[] { code })
                .AddMetadataReference(nUnit)
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] { doc }, CancellationToken.None).ConfigureAwait(false);

            var baseResult = CSharpDiagnostic(AG0004AsyncAnalyzer.DiagnosticId);
            VerifyDiagnosticResults(diag, analyzersArray, new[]
            {
                baseResult.WithLocation(11, 29),
            });
        }
        [Test]
        public async Task Sleep_Should_Trigger_Validation()
        {
            var code = $@"
                    using System.Threading;
                    using System.Threading.Tasks;
                    
                    public class TestClass
                    {{
	                    public void Test()
	                    {{
		                     {{
			                    var a = new Task(() => {{}});
			                    Task.WaitAny(a);
		                    }}
	                  }}
                   }}

            ";

            var nUnit = MetadataReference.CreateFromFile(typeof(TestFixtureAttribute).Assembly.Location);

            var doc = CreateProject(new[] { code })
                .AddMetadataReference(nUnit)
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] { doc }, CancellationToken.None).ConfigureAwait(false);

            var baseResult = CSharpDiagnostic(AG0004AsyncAnalyzer.DiagnosticId);
            VerifyDiagnosticResults(diag, analyzersArray, new[]
            {
                baseResult.WithLocation(11, 29),
            });
        }
        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0004AsyncAnalyzer();
        }
    }
}