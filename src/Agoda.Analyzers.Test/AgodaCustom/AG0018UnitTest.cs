// <copyright file="AG018Unittest.cs" company="Agoda Company Co., Ltd.">
// AGODA ® is a registered trademark of AGIP LLC, used under license by Agoda Company Co., Ltd.. Agoda is part of Priceline (NASDAQ:PCLN)
// </copyright>
namespace Agoda.Analyzers.Test.AgodaCustom
{
    using Agoda.Analyzers.AgodaCustom;
    using Agoda.Analyzers.Test.Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class AG0018UnitTest : DiagnosticVerifier
    {
        [Test]
        public async Task AG0018_WhenCreateAMethodWhichReturnList_ShouldReturnAsAnInterface()
        {
            var code = $@"
                        namespace Tests
                        {{
                            using System.Collections.Generic;
                            using System.Collections.ObjectModel;
                            public class IServerNamesProvider
                            {{
                                public List<double> LocalServer {{ get; set; }}
                                public IList<double> LocalServerV2 {{ get; set; }}
                                public HashSet<int> GetServerSet() {{ return null; }}
                                public ISet<int> GetServerSetV2() {{ return null; }}
                                public List<int> GetServerCounts() {{ return null; }}
                                public IList<string> GetServerFullNames() {{ return null; }}        
                                public Dictionary<string, string> GetServerDns() {{ return null; }}
                                public IDictionary<string, string> GetServerDnsV2() {{ return null; }}
                                public ReadOnlyDictionary<string, int> GetNumberOfServer() {{ return null; }}        
                                public IReadOnlyDictionary<string, int> GetNumberOfServerV2() {{ return null; }}
                                public KeyedCollection<string, string> GetServerDnsV3() {{ return null; }}
                            }}
                        }}";

            var nUnit = MetadataReference.CreateFromFile(typeof(TestFixtureAttribute).Assembly.Location);

            var doc = CreateProject(new[] { code })
                .AddMetadataReference(nUnit)
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] { doc }, CancellationToken.None).ConfigureAwait(false);
            var baseResult = CSharpDiagnostic(AG0018EnsureThatPubliclyExposedIEnumerableTypes.DiagnosticId);

            VerifyDiagnosticResults(diag, analyzersArray, new[]
            {
                baseResult.WithLocation(8, 33),
                baseResult.WithLocation(10, 33),
                baseResult.WithLocation(12, 33),
                baseResult.WithLocation(14, 33),
                baseResult.WithLocation(16, 33),
            });
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0018EnsureThatPubliclyExposedIEnumerableTypes();
        }
    }
}