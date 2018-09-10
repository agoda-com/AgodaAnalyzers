using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    internal class AG0019UnitTests : DiagnosticVerifier
    {
        [Test]
        public async Task AG0019_RemoveInternalsVisibleToAttributeShouldReportCorrectly()
        {
            var code = @"
                    using System;
                    using System.Diagnostics;
                    using System.Reflection;
                    using System.Runtime.CompilerServices;

                    [assembly: AssemblyTitle(""MyApplication"")]
                    [assembly: InternalsVisibleTo(""Agoda.Website.UnitTestFramework"")]
                    [assembly: AssemblyDescription(""Description""), InternalsVisibleTo(""Agoda.Website.UnitTestFramework"")]

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

            var doc = CreateProject(new[] { code }).Documents.First();
            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();
            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] { doc }, CancellationToken.None)
                .ConfigureAwait(false);

            var expected = CSharpDiagnostic(AG0019PreventUseOfInternalsVisibleToAttribute.DIAGNOSTIC_ID);

            VerifyDiagnosticResults(diag, analyzersArray, new[] {
                expected.WithLocation(8, 32),
                expected.WithLocation(9, 68)
            });
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0019PreventUseOfInternalsVisibleToAttribute();
        }
    }
}
