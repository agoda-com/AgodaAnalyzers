using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom;

internal class AG0020FixProviderUnitTests : CodeFixVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0020AvoidReturningNullEnumerables();
        
    protected override string DiagnosticId => AG0020AvoidReturningNullEnumerables.DIAGNOSTIC_ID;
        
    protected override CodeFixProvider CodeFixProvider => new AG0020AvoidReturningNullEnumerablesFixProvider();
        
    [Test]
    public async Task TestShouldFixIEnumerableCorrectly()
    {
        var code = @"
                using System.Collections.Generic;
                using System.Linq;
                
                namespace Agoda.Analyzers.Test
                {
                    public class TestClass
                    {
                        public IEnumerable<string> GetValuesForId(int[] ids)
                        {
                            if (!ids.Any())
                                return null;
                            return null;
                        }
                    }
                }
            ";
            
        var result = @"
                using System.Collections.Generic;
                using System.Linq;
                
                namespace Agoda.Analyzers.Test
                {
                    public class TestClass
                    {
                        public IEnumerable<string> GetValuesForId(int[] ids)
                        {
                            if (!ids.Any())
                                return Enumerable.Empty<string>();
                            return Enumerable.Empty<string>();
                        }
                    }
                }
            ";

        await VerifyCodeFixAsync(code, result);
    }

    [Test]
    public async Task TestShouldFixListCorrectly()
    {
        var code = @"
                using System.Collections.Generic;
                
                namespace Agoda.Analyzers.Test
                {
                    public class TestClass
                    {
                        public List<string> GetValuesForId(int id)
                        {
                            return null;
                        }
                    }
                }
            ";
            
        var result = @"
                using System.Collections.Generic;
                
                namespace Agoda.Analyzers.Test
                {
                    public class TestClass
                    {
                        public List<string> GetValuesForId(int id)
                        {
                            return new List<string>();
                        }
                    }
                }
            ";

        await VerifyCodeFixAsync(code, result);
    }

    [Test]
    public async Task TestShouldFixArrayCorrectly()
    {
        var code = @"
                using System;
                using System.Collections.Generic;
                
                namespace Agoda.Analyzers.Test
                {
                    public class TestClass
                    {
                        public Int32[] GetValuesForId(int id)
                        {
                            return null;
                        }
                    }
                }
            ";
            
        var result = @"
                using System;
                using System.Collections.Generic;
                
                namespace Agoda.Analyzers.Test
                {
                    public class TestClass
                    {
                        public Int32[] GetValuesForId(int id)
                        {
                            return Array.Empty<Int32>();
                        }
                    }
                }
            ";

        await VerifyCodeFixAsync(code, result);
    }

    [Test]
    public async Task TestShouldFixLinkedListCorrectly()
    {
        var code = @"
                using System.Collections.Generic;
                
                namespace Agoda.Analyzers.Test
                {
                    public class TestClass
                    {
                        public LinkedList<string> GetValuesForId(int id)
                        {
                            return null;
                        }
                    }
                }
            ";
            
        var result = @"
                using System.Collections.Generic;
                
                namespace Agoda.Analyzers.Test
                {
                    public class TestClass
                    {
                        public LinkedList<string> GetValuesForId(int id)
                        {
                            return new LinkedList<string>();
                        }
                    }
                }
            ";

        await VerifyCodeFixAsync(code, result);
    }
}