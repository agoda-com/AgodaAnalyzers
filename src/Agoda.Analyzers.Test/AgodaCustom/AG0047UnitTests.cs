using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using DotNet.Testcontainers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;
using Testcontainers.PostgreSql;

namespace Agoda.Analyzers.Test.AgodaCustom;

[TestFixture]
class AG0047UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0047TestContainerBuildOrderAnalyzer();

    protected override string DiagnosticId => AG0047TestContainerBuildOrderAnalyzer.DiagnosticId;

    [Test]
    public async Task BuildCalledBeforeEnvVarSet_RaisesDiagnostic()
    {
        var code = new CodeDescriptor
        {
            References = new[]
            {
                typeof(System.Environment).Assembly,
                typeof(INetwork).Assembly,
                typeof(PostgreSqlContainer).Assembly,
                typeof(Docker.DotNet.DockerClient).Assembly
            },
            Code = @"
                    using System;
                    using DotNet.Testcontainers;
                    using DotNet.Testcontainers.Networks;
                    using DotNet.Testcontainers.Builders;
                    using Testcontainers.PostgreSql;
                    
                    public class TestClass
                    {
                        private readonly INetwork _network;
                        private readonly PostgreSqlContainer _postgreSqlContainer;

                        public TestClass()
                        {
                            _network = new NetworkBuilder()
                                .WithName(""shared_test_network"")
                                .Build();

                            _postgreSqlContainer = new PostgreSqlBuilder()
                                .WithNetwork(_network)
                                .WithNetworkAliases(""postgres"")
                                .Build();
                        }

                        public void Setup()
                        {
                            Environment.SetEnvironmentVariable(""TESTCONTAINERS_HUB_IMAGE_NAME_PREFIX"", ""YOUR_INTERNAL_MIRROR"");
                        }
                    }"
        };
        
        var expectedLocations = new[]
        {
            new DiagnosticLocation(17, 34),
            new DiagnosticLocation(22, 34)
        };
        
        await VerifyDiagnosticsAsync(code, expectedLocations);
    }

    [Test]
    public async Task BuildCalledAfterEnvVarSet_NoDiagnostic()
    {
        var code = new CodeDescriptor
        {
            References = new[]
            {
                typeof(System.Environment).Assembly,
                typeof(INetwork).Assembly,
                typeof(Docker.DotNet.DockerClient).Assembly
            },
            Code = @"
                    using System;
                    using DotNet.Testcontainers;
                    using DotNet.Testcontainers.Networks;
                    using DotNet.Testcontainers.Builders;
                    
                    public class TestClass
                    {
                        public void Setup()
                        {
                            Environment.SetEnvironmentVariable(""TESTCONTAINERS_HUB_IMAGE_NAME_PREFIX"", ""YOUR_INTERNAL_MIRROR"");
                            
                            var network = new NetworkBuilder()
                                .WithName(""shared_test_network"")
                                .Build();
                        }
                    }"
        };
        
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task BuildCalledInDifferentMethod_NoDiagnostic()
    {
        var code = new CodeDescriptor
        {
            References = new[]
            {
                typeof(System.Environment).Assembly,
                typeof(INetwork).Assembly,
                typeof(Docker.DotNet.DockerClient).Assembly
            },
            Code = @"
                    using System;
                    using DotNet.Testcontainers;
                    using DotNet.Testcontainers.Networks;
                    using DotNet.Testcontainers.Builders;
                    
                    public class TestClass
                    {
                        public void Setup()
                        {
                            Environment.SetEnvironmentVariable(""TESTCONTAINERS_HUB_IMAGE_NAME_PREFIX"", ""YOUR_INTERNAL_MIRROR"");
                        }

                        public void CreateNetwork()
                        {
                            var network = new NetworkBuilder()
                                .WithName(""shared_test_network"")
                                .Build();
                        }
                    }"
        };
        
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task NonTestContainerBuild_NoDiagnostic()
    {
        var code = new CodeDescriptor
        {
            References = new[]
            {
                typeof(System.Environment).Assembly
            },
            Code = @"
                    using System;
                    
                    public class TestClass
                    {
                        public void Setup()
                        {
                            var builder = new Builder();
                            builder.Build();
                        }
                    }

                    public class Builder
                    {
                        public void Build() { }
                    }"
        };
        
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task BuildCalledAfterOneTimeSetUp_NoDiagnostic()
    {
        var code = new CodeDescriptor
        {
            References = new[]
            {
                typeof(System.Environment).Assembly,
                typeof(INetwork).Assembly,
                typeof(Docker.DotNet.DockerClient).Assembly,
                typeof(NUnit.Framework.OneTimeSetUpAttribute).Assembly
            },
            Code = @"
                    using System;
                    using NUnit.Framework;
                    using DotNet.Testcontainers;
                    using DotNet.Testcontainers.Networks;
                    using DotNet.Testcontainers.Builders;
                    
                    public class TestClass
                    {
                        private INetwork _network;

                        [OneTimeSetUp]
                        public void OneTimeSetUp()
                        {
                            Environment.SetEnvironmentVariable(""TESTCONTAINERS_HUB_IMAGE_NAME_PREFIX"", ""YOUR_INTERNAL_MIRROR"");
                        }

                        public void CreateNetwork()
                        {
                            _network = new NetworkBuilder()
                                .WithName(""shared_test_network"")
                                .Build();
                        }
                    }"
        };
        
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task BuildCalledAfterSetUp_NoDiagnostic()
    {
        var code = new CodeDescriptor
        {
            References = new[]
            {
                typeof(System.Environment).Assembly,
                typeof(INetwork).Assembly,
                typeof(Docker.DotNet.DockerClient).Assembly,
                typeof(NUnit.Framework.SetUpAttribute).Assembly
            },
            Code = @"
                    using System;
                    using NUnit.Framework;
                    using DotNet.Testcontainers;
                    using DotNet.Testcontainers.Networks;
                    using DotNet.Testcontainers.Builders;
                    
                    public class TestClass
                    {
                        private INetwork _network;

                        [SetUp]
                        public void SetUp()
                        {
                            Environment.SetEnvironmentVariable(""TESTCONTAINERS_HUB_IMAGE_NAME_PREFIX"", ""YOUR_INTERNAL_MIRROR"");
                        }

                        public void CreateNetwork()
                        {
                            _network = new NetworkBuilder()
                                .WithName(""shared_test_network"")
                                .Build();
                        }
                    }"
        };
        
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task BuildCalledInConstructorAfterEnvVarSet_NoDiagnostic()
    {
        var code = new CodeDescriptor
        {
            References = new[]
            {
                typeof(System.Environment).Assembly,
                typeof(INetwork).Assembly,
                typeof(Docker.DotNet.DockerClient).Assembly
            },
            Code = @"
                    using System;
                    using DotNet.Testcontainers;
                    using DotNet.Testcontainers.Networks;
                    using DotNet.Testcontainers.Builders;
                    
                    public class TestClass
                    {
                        private readonly INetwork _network;

                        public TestClass()
                        {
                            Environment.SetEnvironmentVariable(""TESTCONTAINERS_HUB_IMAGE_NAME_PREFIX"", ""YOUR_INTERNAL_MIRROR"");
                            
                            _network = new NetworkBuilder()
                                .WithName(""shared_test_network"")
                                .Build();
                        }
                    }"
        };
        
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task BuildCalledInConstructorBeforeEnvVarSet_RaisesDiagnostic()
    {
        var code = new CodeDescriptor
        {
            References = new[]
            {
                typeof(System.Environment).Assembly,
                typeof(INetwork).Assembly,
                typeof(Docker.DotNet.DockerClient).Assembly
            },
            Code = @"
                    using System;
                    using DotNet.Testcontainers;
                    using DotNet.Testcontainers.Networks;
                    using DotNet.Testcontainers.Builders;
                    
                    public class TestClass
                    {
                        private readonly INetwork _network;

                        public TestClass()
                        {
                            _network = new NetworkBuilder()
                                .WithName(""shared_test_network"")
                                .Build();

                            Environment.SetEnvironmentVariable(""TESTCONTAINERS_HUB_IMAGE_NAME_PREFIX"", ""YOUR_INTERNAL_MIRROR"");
                        }
                    }"
        };
        
        var expectedLocations = new[]
        {
            new DiagnosticLocation(15, 34)
        };
        
        await VerifyDiagnosticsAsync(code, expectedLocations);
    }
} 