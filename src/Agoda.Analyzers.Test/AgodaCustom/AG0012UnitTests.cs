using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using System.Threading.Tasks;
using FluentAssertions;

namespace Agoda.Analyzers.Test.AgodaCustom;

[TestFixture]
internal class AG0012UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0012TestMethodMustContainAtLeastOneAssertion();

    protected override string DiagnosticId => AG0012TestMethodMustContainAtLeastOneAssertion.DIAGNOSTIC_ID;

    [Test]
    public async Task AG0012_WithSetupMethod_ShouldNotShowWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(TestFixtureAttribute).Assembly },
            Code = @"
                    using NUnit.Framework;
                    using System;
                    
                    namespace Tests
                    {
                        public class TestClass
                        {
                            [SetUp]
                            public void This_Is_Valid()
                            {
                                int[] arrayForShouldBe = { 1, 2, 3 };
                            }
                        }
                    }"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0012_WithNoAssertion_ShouldShowWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(TestFixtureAttribute).Assembly },
            Code = @"
                    using NUnit.Framework;
                    using System;
                    
                    namespace Tests
                    {
                        public class TestClass
                        {
                            [Test]
                            public void This_Is_NotValid()
                            {
                                int[] arrayForShouldBe = { 1, 2, 3 };
                            }
                        }
                    }"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(9, 29));
    }

    [Test]
    public async Task AG0012_WithNUnitAssertion_ShouldNotShowWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(TestFixtureAttribute).Assembly, typeof(Shouldly.Should).Assembly },
            Code = @"
                    using NUnit.Framework;
                    
                    namespace Tests
                    {
                        public class TestClass
                        {
                            [Test]
                            public void This_Is_Valid()
                            {
                                int[] arrayToAssert = { 1, 2, 3 };
                                Assert.That(arrayToAssert, Has.Exactly(1).EqualTo(3));
                            }
                        }
                    }"
        };
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }
    
    [Test]
    public async Task AG0012_WithXUnitAssertion_ShouldNotShowWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(Xunit.FactAttribute).Assembly, typeof(Xunit.Assert).Assembly },
            Code = @"
                    using Xunit;

                    namespace Tests
                    {
                        public class TestClass
                        {
                            [Fact]
                            public void This_Is_Valid()
                            {
                                int[] arrayToAssert = { 1, 2, 3 };
                                Assert.NotNull(arrayToAssert);
                            }
                        }
                    }"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0012_WithFluentAssertions_ShouldNotShowWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(NUnitAttribute).Assembly, 
                typeof(FluentAssertions.CallerIdentifier).Assembly
            },
            Code = @"
                    using NUnit.Framework;
                    using FluentAssertions;

                    namespace Tests
                    {
                        internal class TestClass
                        {
                            [Test]
                            internal void This_Is_Valid()
                            {
                                int[] arrayToAssert = { 1, 2, 3 };
                                arrayToAssert.Should().Be(new[]{ 1, 2, 3 });
                            }
                        }
                    }"
        };
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }
    [Test]
    public async Task AG0012_WithShouldlyAssertion_ShouldNotShowWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(TestFixtureAttribute).Assembly, typeof(Shouldly.Should).Assembly },
            Code = @"
                    using NUnit.Framework;
                    using Shouldly;
                    using System;
                    
                    namespace Tests
                    {
                        public class TestClass
                        {
                            [Test]
                            public void This_Is_Valid()
                            {
                                int[] arrayForShouldBe = { 1, 2, 3 };
                                arrayForShouldBe.Length.ShouldBe(3);
                            }
                    
                            [Test]
                            public void This_Is_AlsoValid()
                            {
                                Should.Throw<Exception>(() => {
                                    var y = 1;
                                });
                            }
                        }
                    }"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0012_WithNestedAssertions_ShouldNotShowWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(TestFixtureAttribute).Assembly, typeof(Shouldly.Should).Assembly },
            Code = @"
                    using NUnit.Framework;
                    using Shouldly;
                    using System;
                    
                    namespace Tests
                    {
                        public class TestClass
                        {
                            [Test]
                            public void Assertion_In_Loop_Is_Valid()
                            {
                                int[] array = { 1, 2, 3 };
                                for (int i = 0; i < 3; i++)
                                {
                                    array[i].ShouldBe(i + 1);
                                }
                            }
                            
                            [Test]
                            public void Assertion_In_Nested_Loop_Is_Valid()
                            {
                                int[] array = { 1, 2, 3 };
                                for (int i = 0; i < 3; i++)
                                {
                                    for (int j = 0; j < 3; j++)
                                    {    
                                        array[i].ShouldBe(i + 1);
                                    }
                                }
                            }

                            [Test]
                            public void Assertion_In_Local_Function_Is_Valid()
                            {
                                int[] array = { 1, 2, 3 };
                                
                                helper(array[0]);

                                void helper(int i)
                                {
	                                Assert.AreEqual(i, i + 1);
                                }
                            }
                        }
                    }"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }
}