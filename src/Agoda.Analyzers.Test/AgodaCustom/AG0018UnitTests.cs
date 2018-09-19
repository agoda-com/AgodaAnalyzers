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

    public class AG0018UnitTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0018PermitOnlyCertainPubliclyExposedEnumerables();
        
        protected override string DiagnosticId => AG0018PermitOnlyCertainPubliclyExposedEnumerables.DIAGNOSTIC_ID;
        
        [Test]
        public async Task AG0018_ShouldBeAllowedWhenCreateAMethodWhichReturnInterfaces()
        {
            var code = @"namespace Tests
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
                                public int NumberOfServer { get; set; }
                                public byte[] RawData { get; set; }
                                public string DNSName { get; set; }
                            }
                        }";

            await VerifyDiagnosticResults(code, typeof(TestFixtureAttribute).Assembly);
        }

        [Test]
        public async Task AG0018_ShouldNotBeAllowedWhenCreateAMethodWhichReturnImplementedClass()
        {
            var code = @"namespace Tests
                        {
                            using System.Collections.Generic;
                            using System.Collections.ObjectModel;
                            public class ServerNamesProvider
                            {
                                public List<double> LocalServer { get; set; }
                                public HashSet<int> GetServerSet() { return null; }
                                public List<int> GetServerCounts() { return null; }
                                public Dictionary<string, string> GetServerDns() { return null; }
                                public ReadOnlyDictionary<string, int> GetNumberOfServer() { return null; }  
                                public int[] ServerPerCluster { get; set; }
                            }
                        }";

            var expected = new[]
            {
                new DiagnosticLocation(7, 33),
                new DiagnosticLocation(8, 33),
                new DiagnosticLocation(9, 33),
                new DiagnosticLocation(10, 33),
                new DiagnosticLocation(11, 33),
                new DiagnosticLocation(12, 33),
            };
            await VerifyDiagnosticResults(code, expected);
        }

        [Test]
        public async Task AG0018_WhenCreateAMethodInAnInterfaceWhichReturnList_ShouldReturnAsAnInterface()
        {
            var code = @"namespace Tests
                        {
                            using System.Collections.Generic;
                            using System.Collections.ObjectModel;
                            public interface IServerNamesProvider
                            {
                                HashSet<int> GetServerSet();
                                ISet<int> GetServerSetV2();
                                List<int> GetServerCounts();
                                IList<string> GetServerFullNames();      
                                Dictionary<string, string> GetServerDns();
                                IDictionary<string, string> GetServerDnsV2();
                                ReadOnlyDictionary<string, int> GetNumberOfServer();
                                IReadOnlyDictionary<string, int> GetNumberOfServerV2();
                                KeyedCollection<string, string> GetServerDnsV3();
                            }
                        }";

            var expected = new[]
            {
                new DiagnosticLocation(7, 33),
                new DiagnosticLocation(9, 33),
                new DiagnosticLocation(11, 33),
                new DiagnosticLocation(13, 33),
            };
            await VerifyDiagnosticResults(code, expected);
        }

        [Test]
        public async Task AG0018_WhenCreateAMethodWhichReturnList_ShouldNotEffectOtherCases()
        {
            var code = @"namespace Tests
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
                        }";

            await VerifyDiagnosticResults(code, new DiagnosticLocation(7, 33));
        }
    }
}