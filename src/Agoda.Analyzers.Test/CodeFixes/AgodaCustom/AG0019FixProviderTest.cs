using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.CodeFixes.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Agoda.Analyzers.Test.CodeFixes.AgodaCustom
{
    internal class AG0019FixProviderTest : CodeFixVerifier
    {
        [Test]
        public async Task AG0019_ShouldRemoveAttributeCorrectly()
        {
            var code = @"
                    using System;
                    using System.Diagnostics;
                    using System.Reflection;
                    using System.Runtime.CompilerServices;

                    [assembly: AssemblyTitle(""MyApplication"")]
                    [assembly: InternalsVisibleTo(""Agoda.Website.UnitTestFramework"")]
                    [assembly: AssemblyDescription(""Description""), InternalsVisibleTo(""Agoda.Website.UnitTestFramework"")]
                    [assembly: InternalsVisibleTo(""Agoda.Website.UnitTestFramework""), AssemblyDefaultAlias(""alias"")]
                    [assembly: AssemblyCopyright(""CopyRight""), InternalsVisibleTo(""Agoda.Website.UnitTestFramework""), InternalsVisibleTo(""Agoda.Website.UnitTestFramework""), AssemblyFileVersion(""0.0.0.0"")]

                    namespace RoslynTest
                        {
                            [Serializable]
                            public class Program
                            {
                                [Conditional(""DEBUG""), Conditional(""TEST1"")]
                                static void Main(string[] args)
                                {
                                    Console.WriteLine(""Hello World!"");
                                }
                            }
                        }
                    ";

            var result = @"
                    using System;
                    using System.Diagnostics;
                    using System.Reflection;
                    using System.Runtime.CompilerServices;

                    [assembly: AssemblyTitle(""MyApplication"")]
                    
                    [assembly: AssemblyDescription(""Description"")]
                    [assembly: AssemblyDefaultAlias(""alias"")]
                    [assembly: AssemblyCopyright(""CopyRight""),AssemblyFileVersion(""0.0.0.0"")]

                    namespace RoslynTest
                        {
                            [Serializable]
                            public class Program
                            {
                                [Conditional(""DEBUG""), Conditional(""TEST1"")]
                                static void Main(string[] args)
                                {
                                    Console.WriteLine(""Hello World!"");
                                }
                            }
                        }
                    ";

            var doc = CreateProject(new[] { code })
                .Documents
                .First();
            var analyzerArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();
            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzerArray, new[] { doc }, CancellationToken.None)
                .ConfigureAwait(false);
            var expected = CSharpDiagnostic(AG0019PreventUseOfInternalsVisibleToAttribute.DIAGNOSTIC_ID);
            VerifyDiagnosticResults(diag, analyzerArray, new[] {
                expected.WithLocation(8, 32),
                expected.WithLocation(9, 68),
                expected.WithLocation(10, 32),
                expected.WithLocation(11, 64),
                expected.WithLocation(11, 119)
            });
            await VerifyCSharpFixAsync(code, result, allowNewCompilerDiagnostics: true);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider() => new AG0019FixProvider();

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0019PreventUseOfInternalsVisibleToAttribute();
        }
    }
}
