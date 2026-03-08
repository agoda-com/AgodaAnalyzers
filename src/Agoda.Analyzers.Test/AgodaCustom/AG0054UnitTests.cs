using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    [TestFixture]
    internal class AG0054UnitTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0054DetectSplitBindingStepDefinitions();
        protected override string DiagnosticId => AG0054DetectSplitBindingStepDefinitions.DIAGNOSTIC_ID;

        private const string SpecFlowStub = @"
namespace TechTalk.SpecFlow
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class BindingAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class GivenAttribute : System.Attribute
    {
        public GivenAttribute(string pattern) { }
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class WhenAttribute : System.Attribute
    {
        public WhenAttribute(string pattern) { }
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ThenAttribute : System.Attribute
    {
        public ThenAttribute(string pattern) { }
    }
}";

        private const string ReqnrollStub = @"
namespace Reqnroll
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class BindingAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class GivenAttribute : System.Attribute
    {
        public GivenAttribute(string pattern) { }
    }
}";

        [Test]
        public async Task AG0054_WhenBindingPartialClassSplitAcrossFiles_ShowError()
        {
            var file1 = @"
using TechTalk.SpecFlow;

[Binding]
public partial class OrderSteps
{
    [Given(@""a customer with id '(.*)'"")]
    public void GivenACustomerWithId(string customerId) { }

    [When(@""the customer places an order"")]
    public void WhenTheCustomerPlacesAnOrder() { }
}";

            var file2 = @"
using TechTalk.SpecFlow;

[Binding]
public partial class OrderSteps
{
    [Then(@""the order status should be '(.*)'"")]
    public void ThenTheOrderStatusShouldBe(string expectedStatus) { }
}";

            await VerifyMultiFileDiagnosticsAsync(
                new[] { SpecFlowStub, file1, file2 },
                expectedCount: 1,
                expectedClassName: "OrderSteps",
                expectedFileCount: "2");
        }

        [Test]
        public async Task AG0054_WhenBindingPartialClassSplitAcrossThreeFiles_ShowError()
        {
            var file1 = @"
using TechTalk.SpecFlow;

[Binding]
public partial class OrderSteps
{
    [Given(@""a customer with id '(.*)'"")]
    public void GivenACustomerWithId(string customerId) { }
}";

            var file2 = @"
using TechTalk.SpecFlow;

[Binding]
public partial class OrderSteps
{
    [When(@""the customer places an order"")]
    public void WhenTheCustomerPlacesAnOrder() { }
}";

            var file3 = @"
using TechTalk.SpecFlow;

[Binding]
public partial class OrderSteps
{
    [Then(@""the order total should be (.*)"")]
    public void ThenTheOrderTotalShouldBe(decimal expectedTotal) { }
}";

            await VerifyMultiFileDiagnosticsAsync(
                new[] { SpecFlowStub, file1, file2, file3 },
                expectedCount: 1,
                expectedClassName: "OrderSteps",
                expectedFileCount: "3");
        }

        [Test]
        public async Task AG0054_WhenReqnrollBindingPartialClassSplitAcrossFiles_ShowError()
        {
            var file1 = @"
using Reqnroll;

[Binding]
public partial class OrderSteps
{
    [Given(@""a customer with id '(.*)'"")]
    public void GivenACustomerWithId(string customerId) { }
}";

            var file2 = @"
using Reqnroll;

[Binding]
public partial class OrderSteps
{
    public void SomeOtherMethod() { }
}";

            await VerifyMultiFileDiagnosticsAsync(
                new[] { ReqnrollStub, file1, file2 },
                expectedCount: 1,
                expectedClassName: "OrderSteps",
                expectedFileCount: "2");
        }

        [Test]
        public async Task AG0054_WhenAllStepsInSingleFile_NoError()
        {
            var code = new CodeDescriptor
            {
                Code = SpecFlowStub + @"
using TechTalk.SpecFlow;

[Binding]
public class OrderSteps
{
    [Given(@""a customer with id '(.*)'"")]
    public void GivenACustomerWithId(string customerId) { }

    [When(@""the customer places an order"")]
    public void WhenTheCustomerPlacesAnOrder() { }

    [Then(@""the order status should be '(.*)'"")]
    public void ThenTheOrderStatusShouldBe(string expectedStatus) { }
}"
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0054_WhenPartialClassWithoutBindingAttribute_NoError()
        {
            var sources = new[]
            {
                SpecFlowStub,
                @"
public partial class OrderSteps
{
    public void MethodA() { }
}",
                @"
public partial class OrderSteps
{
    public void MethodB() { }
}"
            };

            await VerifyMultiFileDiagnosticsAsync(sources, expectedCount: 0);
        }

        [Test]
        public async Task AG0054_WhenBindingClassNotPartial_NoError()
        {
            var code = new CodeDescriptor
            {
                Code = SpecFlowStub + @"
using TechTalk.SpecFlow;

[Binding]
public class OrderSteps
{
    [Given(@""a customer with id '(.*)'"")]
    public void GivenACustomerWithId(string customerId) { }
}"
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0054_WhenPartialClassInSameFile_NoError()
        {
            var code = new CodeDescriptor
            {
                Code = SpecFlowStub + @"
using TechTalk.SpecFlow;

[Binding]
public partial class OrderSteps
{
    [Given(@""a customer with id '(.*)'"")]
    public void GivenACustomerWithId(string customerId) { }

    [When(@""the customer places an order"")]
    public void WhenTheCustomerPlacesAnOrder() { }
}"
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0054_WhenSeparateBindingClassesInDifferentFiles_NoError()
        {
            var sources = new[]
            {
                SpecFlowStub,
                @"
using TechTalk.SpecFlow;

[Binding]
public class OrderGivenSteps
{
    [Given(@""a customer with id '(.*)'"")]
    public void GivenACustomerWithId(string customerId) { }
}",
                @"
using TechTalk.SpecFlow;

[Binding]
public class OrderThenSteps
{
    [Then(@""the order status should be '(.*)'"")]
    public void ThenTheOrderStatusShouldBe(string expectedStatus) { }
}"
            };

            await VerifyMultiFileDiagnosticsAsync(sources, expectedCount: 0);
        }

        private async Task VerifyMultiFileDiagnosticsAsync(
            string[] sources,
            int expectedCount,
            string expectedClassName = null,
            string expectedFileCount = null)
        {
            var project = CreateProject(sources);
            var documents = project.Documents.ToArray();
            var analyzersArray = ImmutableArray.Create(DiagnosticAnalyzer);
            var diagnostics = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, documents, CancellationToken.None);

            Assert.AreEqual(expectedCount, diagnostics.Length,
                $"Expected {expectedCount} diagnostics but got {diagnostics.Length}. " +
                $"Diagnostics: {string.Join(", ", diagnostics.Select(d => d.ToString()))}");

            if (expectedCount > 0 && expectedClassName != null)
            {
                var diag = diagnostics.First();
                Assert.AreEqual(AG0054DetectSplitBindingStepDefinitions.DIAGNOSTIC_ID, diag.Id);
                StringAssert.Contains(expectedClassName, diag.GetMessage());
                if (expectedFileCount != null)
                {
                    StringAssert.Contains(expectedFileCount, diag.GetMessage());
                }
            }
        }
    }
}
