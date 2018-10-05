using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    internal class AG0020UnitTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0020AvoidReturningNullEnumerables();

        protected override string DiagnosticId => AG0020AvoidReturningNullEnumerables.DIAGNOSTIC_ID;

        [Test]
        public async Task PreventReturningNullForReturnValueOfIEnumerable_ShouldReportCorrectly()
        {

            var code = @"
using System.Collections.Generic;

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

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(10, 20));
        }

        [Test]
        public async Task
            PreventReturningNullForReturnValueOfIEnumerable_ShouldReportCorrectlyForClassImplementingIEnumerable()
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

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(10, 20));
        }

        [Test]
        public async Task PreventReturningNullForReturnValueOfIEnumerable_ShouldNotReportStringReturnType()
        {
            var code = @"
using System.Collections.Generic;

namespace Agoda.Analyzers.Test
{
    public class TestClass
    {
        public string GetValuesForId(int id)
        {
            return null;
        }
    }
}";

            var doc = CreateProject(new[] {code})
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] {doc}, CancellationToken.None)
                .ConfigureAwait(false);

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task PreventReturningNullForReturnValueOfIEnumerable_ShouldNotReportNullableInt()
        {
            var code = @"
using System.Collections.Generic;

namespace Agoda.Analyzers.Test
{
    public class TestClass
    {
        public int? GetValuesForId(int id)
        {
            return null;
        }
    }
}";

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task PreventReturningNullForReturnValueOfIEnumerable_ShouldReportNullReturnFromProperty()
        {
            var code = @"
using System.Collections.Generic;

namespace Agoda.Analyzers.Test
{
    public class TestClass
    {
        public IEnumerable<string> Data {
            get {
                return null;
            }
        }
        public IEnumerable<string> SecondProperty => null;
    }
}";

            await VerifyDiagnosticsAsync(code, new[]
            {
                new DiagnosticLocation(10, 24),
                new DiagnosticLocation(13, 54)
            });
        }

        [Test]
        public async Task
            PreventReturningNullForReturnValueOfIEnumerable_ShouldNotReportNullReturnFromPropertyOfStringOrOtherType()
        {
            var code = @"
using System.Collections.Generic;

namespace Agoda.Analyzers.Test
{
    public class TestClass
    {
        public string ThisIsOk => null;
        public int? SomeOptionalNumber => null;
    }
}";

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0020_ForNullStringArray_ShowsWarning()
        {
            var code = @"
                namespace Agoda.Analyzers.Test
                {
                    public class TestClass
                    {
                        public string[] Method()
                        {
                            return null;
                        }
                    }
                }";

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(8, 20));
        }

        [Test]
        public async Task AG0020_ForIssue110_DoesNotShowWarning()
        {
            var code = new CodeDescriptor
            {
                References = new[] {typeof(List<>).Assembly, typeof(object).Assembly},
                Code = @"
                    using System;
                    using System.Collections.Generic;
                    
                    namespace Agoda.Analyzers.Test
                    {                    
                        public class TestClass
                        {                            
                            public static List<object> GetList()
                            {
                                var o = true ? new object() : null;                    
                                var someThings = new List<object> {o};                    
                                return someThings;
                            }
                        }
                    }"
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }
    }
}