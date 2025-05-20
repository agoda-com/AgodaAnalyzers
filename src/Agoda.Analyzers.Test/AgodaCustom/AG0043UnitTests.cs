using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;

namespace Agoda.Analyzers.Test.AgodaCustom;

[TestFixture]
[Parallelizable(ParallelScope.All)]
class AG0043UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0043NoBuildServiceProvider();

    protected override string DiagnosticId => AG0043NoBuildServiceProvider.DIAGNOSTIC_ID;

    [Test]
    public async Task BuildServiceProvider_Used_RaisesDiagnostic()
    {
        var services = new ServiceCollection();
        services.BuildServiceProvider();
        
        var code = new CodeDescriptor
        {
            References = new[]
            {
                typeof(ServiceCollection).Assembly, 
                typeof(ServiceProvider).Assembly, 
                typeof(IServiceCollection).Assembly
            },
            Code = @"
                    using Microsoft.Extensions.DependencyInjection;
                    
                    public class Program
                    {
                        public static void Main()
                        {
                            var services = new ServiceCollection();
                            var provider = services.BuildServiceProvider();
                        }
                    }"
        };
        
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(9, 53));
    }

    [Test]
    public async Task OtherMethod_NoDiagnostic()
    {
        var code = new CodeDescriptor
        {
            References = new[] {typeof(ServiceCollection).Assembly},
            Code = @"
                    using Microsoft.Extensions.DependencyInjection;

                    class TestClass
                    {
                        public void ConfigureServices()
                        {
                            var services = new ServiceCollection();
                            services.AddSingleton<object>();
                        }
                    }"
        };
        
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }
    
    [Test]
    public async Task BuildServiceProvider_WhenChaining_RaiseDiagnostic()
    {
        var code = new CodeDescriptor
        {
            References = new[]
            {
                typeof(ServiceCollection).Assembly, 
                typeof(ServiceProvider).Assembly, 
                typeof(IServiceCollection).Assembly
            },
            Code = @"
                    using Microsoft.Extensions.DependencyInjection;
                    
                    public class Program
                    {
                        public static void Main()
                        {
                            var provider = new ServiceCollection()
                                .AddSingleton<object>()
                                .BuildServiceProvider();
                        }
                    }"
        };
        
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(10, 34));
    }
}