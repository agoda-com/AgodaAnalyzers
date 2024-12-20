﻿
namespace Agoda.Analyzers.Test.AgodaCustom;

using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using System.Threading.Tasks;

[TestFixture]
public class AG0018UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0018PermitOnlyCertainPubliclyExposedEnumerables();
        
    protected override string DiagnosticId => AG0018PermitOnlyCertainPubliclyExposedEnumerables.DIAGNOSTIC_ID;
        
    [Test]
    public async Task AG0018_ShouldBeAllowedWhenCreateAMethodWhichReturnInterfaces()
    {
        var code = new CodeDescriptor
        {
            References = new[] {typeof(TestFixtureAttribute).Assembly},
            Code = @"
                    namespace Tests
                    {
                        using System.Collections.Generic;
                        using System.Collections.ObjectModel;
                        public class ServerNamesProvider
                        {
                            public IList<double> LocalServerV2 { get; set; }
                            public ISet<int> GetServerSetV2() { return null; }
                            public IList<string> GetServerFullNames() { return null; }        
                            public IDictionary<string, string> GetServerDnsV2() { return null; }
                            public IReadOnlyDictionary<string, int> GetNumberOfServerV2() { return null; }
                            public KeyedCollection<string, string> GetServerDnsV3() { return null; }
                            public byte[] GetRawData() { return null; }
                            public int NumberOfServer { get; set; }
                            public byte[] RawData { get; set; }
                            public string DNSName { get; set; }
                        }
                    }"
        };
            
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [TestCase("List<double>")]
    [TestCase("HashSet<int>")]
    [TestCase("Dictionary<string, string>")]
    [TestCase("ReadOnlyDictionary<string, int>")]
    [TestCase("int[]")]
    public async Task AG0018_WithForbiddenCollection_ShowShowWarning(string type)
    {
        var code = $@"
                using System.Collections.Generic;
                using System.Collections.ObjectModel;
                namespace Tests
                {{  
                    public class TestClass
                    {{
                        public {type} Type {{ get; set; }}
                    }}
                }}";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(8, 25));
    }

    [TestCase("ISet<int>")]
    [TestCase("IList<int>")]
    [TestCase("IDictionary<int, int>")]
    [TestCase("byte[]")]
    [TestCase("string")]
    [TestCase("IReadOnlyDictionary<string, int>")]
    [TestCase("KeyedCollection<string, string>")]
    [TestCase("IEnumerable<string>")]
    [TestCase("IReadOnlyCollection<string>")]
    [TestCase("IReadOnlyList<string>")]
    public async Task AG0018_WithPermittedCollection_ShouldNotShowWarning(string type)
    {
        var code = $@"
                namespace Tests
                {{
                    using System.Collections.Generic;
                    using System.Collections.ObjectModel;
                    public interface IServerNamesProvider
                    {{
                        {type} Type {{ get; set; }}
                    }}
                }}";

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0018_WhenCreateAMethodWhichReturnList_ShouldNotEffectOtherCases()
    {
        var code = new CodeDescriptor
        {
            References = new[] {typeof(TestFixtureAttribute).Assembly},
            Code = @"
                    namespace Tests
                    {
                        using System.Collections.Generic;
                        using System.Collections.ObjectModel;
                        public class ServerNamesProvider
                        {
                            public List<double> LocalServer { get; set; }
                            private List<int> GetIPFromFile() { return null; }
                            protected List<int> GetIPFromLocalFile() { return null; }
                            internal List<int> GetIPFromLocalDisk() { return null; }
                        }
                    }"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(8, 29));
    }
}