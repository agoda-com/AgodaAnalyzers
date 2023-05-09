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

namespace Agoda.Analyzers.Test.CodeFixes.AgodaCustom;

internal class AG0019FixProviderUnitTests : CodeFixVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0019PreventUseOfInternalsVisibleToAttribute();
        
    protected override string DiagnosticId => AG0019PreventUseOfInternalsVisibleToAttribute.DIAGNOSTIC_ID;
        
    protected override CodeFixProvider CodeFixProvider => new AG0019PreventUseOfInternalsVisibleToAttributeFixProvider();
        
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
        await VerifyCodeFixAsync(code, result, allowNewCompilerDiagnostics:true);
    }
}