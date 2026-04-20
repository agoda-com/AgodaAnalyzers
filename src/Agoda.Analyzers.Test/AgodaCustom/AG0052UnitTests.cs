using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    [TestFixture]
    internal class AG0052UnitTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0052PreventHardcodedTaskDelayInTests();

        protected override string DiagnosticId => AG0052PreventHardcodedTaskDelayInTests.DIAGNOSTIC_ID;

        [Test]
        public async Task AG0052_WithHardcodedIntLiteral_InTestMethod_ShowsWarning()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(NUnit.Framework.TestFixtureAttribute).Assembly },
                Code = @"
                using System.Threading.Tasks;
                using NUnit.Framework;

                [TestFixture]
                class TestClass
                {
                    [Test]
                    public async Task MyTest()
                    {
                        await Task.Delay(1000);
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(11, 31));
        }

        [Test]
        public async Task AG0052_WithTimeSpanFromSeconds_InTestMethod_ShowsWarning()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(NUnit.Framework.TestFixtureAttribute).Assembly },
                Code = @"
                using System;
                using System.Threading.Tasks;
                using NUnit.Framework;

                [TestFixture]
                class TestClass
                {
                    [Test]
                    public async Task MyTest()
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5));
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(12, 31));
        }

        [Test]
        public async Task AG0052_WithTimeSpanFromMilliseconds_InTestMethod_ShowsWarning()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(NUnit.Framework.TestFixtureAttribute).Assembly },
                Code = @"
                using System;
                using System.Threading.Tasks;
                using NUnit.Framework;

                [TestFixture]
                class TestClass
                {
                    [Test]
                    public async Task MyTest()
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500));
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(12, 31));
        }

        [Test]
        public async Task AG0052_WithNewTimeSpan_InTestMethod_ShowsWarning()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(NUnit.Framework.TestFixtureAttribute).Assembly },
                Code = @"
                using System;
                using System.Threading.Tasks;
                using NUnit.Framework;

                [TestFixture]
                class TestClass
                {
                    [Test]
                    public async Task MyTest()
                    {
                        await Task.Delay(new TimeSpan(0, 0, 5));
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(12, 31));
        }

        [Test]
        public async Task AG0052_InSetUpMethod_ShowsWarning()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(NUnit.Framework.TestFixtureAttribute).Assembly },
                Code = @"
                using System.Threading.Tasks;
                using NUnit.Framework;

                [TestFixture]
                class TestClass
                {
                    [SetUp]
                    public async Task Setup()
                    {
                        await Task.Delay(5000);
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(11, 31));
        }

        [Test]
        public async Task AG0052_InHelperMethodInsideTestFixture_ShowsWarning()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(NUnit.Framework.TestFixtureAttribute).Assembly },
                Code = @"
                using System.Threading.Tasks;
                using NUnit.Framework;

                [TestFixture]
                class TestClass
                {
                    [Test]
                    public async Task MyTest()
                    {
                        await WaitABit();
                    }

                    private async Task WaitABit()
                    {
                        await Task.Delay(1000);
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(16, 31));
        }

        [Test]
        public async Task AG0052_WithVariable_NoWarning()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(NUnit.Framework.TestFixtureAttribute).Assembly },
                Code = @"
                using System.Threading.Tasks;
                using NUnit.Framework;

                [TestFixture]
                class TestClass
                {
                    [Test]
                    public async Task MyTest()
                    {
                        int delay = 1000;
                        await Task.Delay(delay);
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0052_WithConfigurableTimeSpan_NoWarning()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(NUnit.Framework.TestFixtureAttribute).Assembly },
                Code = @"
                using System;
                using System.Threading.Tasks;
                using NUnit.Framework;

                static class TestSettings
                {
                    public static TimeSpan RateLimitWindow => TimeSpan.FromSeconds(5);
                }

                [TestFixture]
                class TestClass
                {
                    [Test]
                    public async Task MyTest()
                    {
                        await Task.Delay(TestSettings.RateLimitWindow);
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0052_InsideTaskWhenAny_NoWarning()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(NUnit.Framework.TestFixtureAttribute).Assembly },
                Code = @"
                using System;
                using System.Threading.Tasks;
                using NUnit.Framework;

                [TestFixture]
                class TestClass
                {
                    [Test]
                    public async Task MyTest()
                    {
                        var tcs = new TaskCompletionSource<bool>();
                        var completed = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(10)));
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0052_InsideTaskWhenAnyWithIntLiteral_NoWarning()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(NUnit.Framework.TestFixtureAttribute).Assembly },
                Code = @"
                using System.Threading.Tasks;
                using NUnit.Framework;

                [TestFixture]
                class TestClass
                {
                    [Test]
                    public async Task MyTest()
                    {
                        var tcs = new TaskCompletionSource<bool>();
                        var completed = await Task.WhenAny(tcs.Task, Task.Delay(5000));
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0052_InNonTestClass_NoWarning()
        {
            var code = new CodeDescriptor
            {
                Code = @"
                using System.Threading.Tasks;

                class ProductionService
                {
                    public async Task DoWork()
                    {
                        await Task.Delay(1000);
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0052_WithTimeSpanFromMinutes_InTestMethod_ShowsWarning()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(NUnit.Framework.TestFixtureAttribute).Assembly },
                Code = @"
                using System;
                using System.Threading.Tasks;
                using NUnit.Framework;

                [TestFixture]
                class TestClass
                {
                    [Test]
                    public async Task MyTest()
                    {
                        await Task.Delay(TimeSpan.FromMinutes(1));
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(12, 31));
        }

        [Test]
        public async Task AG0052_MultipleViolations_ShowsMultipleWarnings()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(NUnit.Framework.TestFixtureAttribute).Assembly },
                Code = @"
                using System;
                using System.Threading.Tasks;
                using NUnit.Framework;

                [TestFixture]
                class TestClass
                {
                    [Test]
                    public async Task MyTest()
                    {
                        await Task.Delay(1000);
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, new[]
            {
                new DiagnosticLocation(12, 31),
                new DiagnosticLocation(13, 31)
            });
        }

        [Test]
        public async Task AG0052_WithMethodParameter_NoWarning()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(NUnit.Framework.TestFixtureAttribute).Assembly },
                Code = @"
                using System.Threading.Tasks;
                using NUnit.Framework;

                [TestFixture]
                class TestClass
                {
                    [Test]
                    public async Task MyTest()
                    {
                        await DelayFor(1000);
                    }

                    private async Task DelayFor(int ms)
                    {
                        await Task.Delay(ms);
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0052_WithTimeSpanFromVariableSeconds_NoWarning()
        {
            var code = new CodeDescriptor
            {
                References = new[] { typeof(NUnit.Framework.TestFixtureAttribute).Assembly },
                Code = @"
                using System;
                using System.Threading.Tasks;
                using NUnit.Framework;

                [TestFixture]
                class TestClass
                {
                    [Test]
                    public async Task MyTest()
                    {
                        double seconds = 5;
                        await Task.Delay(TimeSpan.FromSeconds(seconds));
                    }
                }"
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }
    }
}
