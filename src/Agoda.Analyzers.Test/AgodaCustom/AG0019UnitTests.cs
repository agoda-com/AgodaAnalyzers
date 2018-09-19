using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    internal class AG0019UnitTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0019PreventUseOfInternalsVisibleToAttribute();
        
        protected override string DiagnosticId => AG0019PreventUseOfInternalsVisibleToAttribute.DIAGNOSTIC_ID;
        
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

            var expected = new[]
            {
                new DiagnosticLocation(8, 32),
                new DiagnosticLocation(9, 68),
                new DiagnosticLocation(10, 32),
                new DiagnosticLocation(11, 64),
                new DiagnosticLocation(11, 119)
            };
            await VerifyDiagnosticResults(code, expected);
        }
    }
}
