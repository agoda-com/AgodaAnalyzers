using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

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
                typeof(System.Environment).Assembly
            },
            Code = @"
                    using System;
                    using Testcontainers;
                    
                    public class TestClass
                    {
                        private readonly INetwork _network = new NetworkBuilder()
                            .WithName(""shared_test_network"")
                            .Build();

                        public void Setup()
                        {
                            Environment.SetEnvironmentVariable(""TESTCONTAINERS_HUB_IMAGE_NAME_PREFIX"", ""YOUR_INTERNAL_MIRROR"");
                        }
                    }"
        };
        
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(8, 29));
    }

    [Test]
    public async Task BuildCalledAfterEnvVarSet_NoDiagnostic()
    {
        var code = new CodeDescriptor
        {
            References = new[]
            {
                typeof(System.Environment).Assembly
            },
            Code = @"
                    using System;
                    using Testcontainers;
                    
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
                typeof(System.Environment).Assembly
            },
            Code = @"
                    using System;
                    using Testcontainers;
                    
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
    public async Task MultipleBuildCalls_RaisesDiagnosticForEach()
    {
        var code = new CodeDescriptor
        {
            References = new[]
            {
                typeof(System.Environment).Assembly
            },
            Code = @"
                    using System;
                    using Testcontainers;
                    
                    public class TestClass
                    {
                        private readonly INetwork _network = new NetworkBuilder()
                            .WithName(""shared_test_network"")
                            .Build();

                        private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
                            .WithNetwork(_network)
                            .WithNetworkAliases(""postgres"")
                            .Build();

                        public void Setup()
                        {
                            Environment.SetEnvironmentVariable(""TESTCONTAINERS_HUB_IMAGE_NAME_PREFIX"", ""YOUR_INTERNAL_MIRROR"");
                        }
                    }"
        };
        
        await VerifyDiagnosticsAsync(code, 
            new DiagnosticLocation(8, 29),
            new DiagnosticLocation(12, 29));
    }
} 