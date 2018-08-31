using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.CodeFixes.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    internal class AG0020FixProviderUnitTests : CodeFixVerifier
    {
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
        public IEnumerable<string> GetValuesForId(int id)
        {
            return null;
        }
    }
}";
            var result = @"
using System.Collections.Generic;
using System.Linq;
namespace Agoda.Analyzers.Test
{
    public class TestClass
    {
        public IEnumerable<string> GetValuesForId(int id)
        {
            return Enumerable.Empty<string>();
        }
    }
}";
            var doc = CreateProject(new[] { code })
                .Documents
                .First();
            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();
            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] { doc }, CancellationToken.None)
                .ConfigureAwait(false);
            var expected = CSharpDiagnostic(AG0020AvoidReturningNullEnumerables.DiagnosticId);
            VerifyDiagnosticResults(diag, analyzersArray, new[] {
                expected.WithLocation(11, 20)
            });
            await VerifyCSharpFixAsync(code, result);
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
}";
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
}";
            var doc = CreateProject(new[] { code })
                .Documents
                .First();
            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();
            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] { doc }, CancellationToken.None)
                .ConfigureAwait(false);
            var expected = CSharpDiagnostic(AG0020AvoidReturningNullEnumerables.DiagnosticId);
            VerifyDiagnosticResults(diag, analyzersArray, new[] {
                expected.WithLocation(10, 20)
            });
            await VerifyCSharpFixAsync(code, result);
        }

        [Test]
        public async Task TestShouldFixArrayCorrectly()
        {
            var code = @"
#pragma warning(disable:CS1591)
using System;
using System.Collections.Generic;

namespace Agoda.Analyzers.Test
{
    public class TestClass
    {
        public string[] GetValuesForId(int id)
        {
            return null;
        }
    }
}";
            var result = @"
#pragma warning(disable:CS1591)
using System;
using System.Collections.Generic;

namespace Agoda.Analyzers.Test
{
    public class TestClass
    {
        public string[] GetValuesForId(int id)
        {
            return Array.Empty<string>();
        }
    }
}";
            var doc = CreateProject(new[] { code })
                .Documents
                .First();
            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();
            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] { doc }, CancellationToken.None)
                .ConfigureAwait(false);
            var expected = CSharpDiagnostic(AG0020AvoidReturningNullEnumerables.DiagnosticId);
            VerifyDiagnosticResults(diag, analyzersArray, new[] {
                expected.WithLocation(12, 20)
            });
            await VerifyCSharpFixAsync(code, result);
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
}";
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
}";
            var doc = CreateProject(new[] { code })
                .Documents
                .First();
            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();
            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] { doc }, CancellationToken.None)
                .ConfigureAwait(false);
            var expected = CSharpDiagnostic(AG0020AvoidReturningNullEnumerables.DiagnosticId);
            VerifyDiagnosticResults(diag, analyzersArray, new[] {
                expected.WithLocation(10, 20)
            });
            await VerifyCSharpFixAsync(code, result);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new AG0020FixProvider();
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0020AvoidReturningNullEnumerables();
        }
    }
}