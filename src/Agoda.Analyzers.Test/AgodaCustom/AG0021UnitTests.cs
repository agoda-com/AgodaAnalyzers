using System.Collections.Generic;
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
    internal class AG0021UnitTests : DiagnosticVerifier
    {
	    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0021PreferAsyncMethods();
        
	    protected override string DiagnosticId => AG0021PreferAsyncMethods.DIAGNOSTIC_ID;
	    
        [Test]
        public async Task AG0021_Static_WhenAsyncDoesNotExist_ShouldNotShowWarning()
        {
            var code = @"
				class TestClassDeclaration {
					public static void TestMethod() {
						int instance = 1;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						TestClassDeclaration.TestMethod();
					}
				}
			";

            await VerifyDiagnosticResults(code);
        }

        [Test]
        public async Task AG0021_Instance_WhenAsyncDoesNotExist_ShouldNotShowWarning()
        {
            var code = @"
				class TestClassDeclaration {
					public void TestMethod() {
						int instance = 1;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						var instance = new TestClassDeclaration();
						instance.TestMethod();
					}
				}
			";

            await VerifyDiagnosticResults(code);
        }
	   

        [Test]
        public async Task AG0021_Static_WhenAsyncWithPostfixExists_ShouldShowWarning()
        {
            var code = @"
				using System.Threading.Tasks;

				class TestClassDeclaration {
					public static void TestMethod() {
						int instance = 1;
					}

					public static Task TestMethodAsync() {
						return Task.CompletedTask;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						TestClassDeclaration.TestMethod();
					}
				}
			";

	        await VerifyDiagnosticResults(code, new DiagnosticLocation(16, 7));
        }

        [Test]
        public async Task AG0021_Instance_WhenAsyncWithPostfixExists_ShouldShowWarning()
        {
            var code = @"
				using System.Threading.Tasks;

				class TestClassDeclaration {
					public void TestMethod() {
						int instance = 1;
					}

					public Task TestMethodAsync() {
						return Task.CompletedTask;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						var instance = new TestClassDeclaration();
						instance.TestMethod();
					}
				}
			";

	        await VerifyDiagnosticResults(code, new DiagnosticLocation(17, 7));
        }

        [Test]
        public async Task AG0021_Static_WhenAsyncWithoutPostfixExists_ShouldShowWarning()
        {
            var code = @"
				using System.Threading;
				using System.Threading.Tasks;

				class TestClassDeclaration {
					public static void TestMethod() {
						int instance = 1;
					}

					public static Task TestMethod(CancellationToken cancellationToken) {
						return Task.CompletedTask;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						TestClassDeclaration.TestMethod();
					}
				}
			";

	        await VerifyDiagnosticResults(code, new DiagnosticLocation(17, 7));
        }

        [Test]
        public async Task AG0021_Instance_WhenAsyncWithoutPostfixExists_ShouldShowWarning()
        {
            var code = @"
				using System.Threading;
				using System.Threading.Tasks;

				class TestClassDeclaration {
					public void TestMethod() {
						int instance = 1;
					}

					public Task TestMethod(CancellationToken cancellationToken) {
						return Task.CompletedTask;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						var instance = new TestClassDeclaration();
						instance.TestMethod();
					}
				}
			";

	        await VerifyDiagnosticResults(code, new DiagnosticLocation(18, 7));
        }

        [Test]
        public async Task AG0021_Static_WhenAsyncWithPostfixIsUsed_ShouldNotShowAnyWarning()
        {
            var code = @"
				using System.Threading.Tasks;

				class TestClassDeclaration {
					public static void TestMethod() {
						int instance = 1;
					}

					public static Task TestMethodAsync() {
						return Task.CompletedTask;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						TestClassDeclaration.TestMethodAsync();
					}
				}
			";

            await VerifyDiagnosticResults(code);
        }

        [Test]
        public async Task AG0021_Instance_WhenAsyncWithPostfixIsUsed_ShouldNotShowAnyWarning()
        {
            var code = @"
				using System.Threading.Tasks;

				class TestClassDeclaration {
					public void TestMethod() {
						int instance = 1;
					}

					public Task TestMethodAsync() {
						return Task.CompletedTask;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						var instance = new TestClassDeclaration();
						instance.TestMethodAsync();
					}
				}
			";

            await VerifyDiagnosticResults(code);
        }

        [Test]
        public async Task AG0021_Static_WhenAsyncWithoutPostfixIsUsed_ShouldNotShowAnyWarning()
        {
            var code = @"
				using System.Threading;
				using System.Threading.Tasks;

				class TestClassDeclaration {
					public static void TestMethod() {
						int instance = 1;
					}

					public static Task TestMethod(CancellationToken cancellationToken) {
						return Task.CompletedTask;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						TestClassDeclaration.TestMethod(CancellationToken.None);
					}
				}
			";

            await VerifyDiagnosticResults(code);
        }

        [Test]
        public async Task AG0021_Instance_WhenAsyncWithoutPostfixIsUsed_ShouldNotShowAnyWarning()
        {
            var code = @"
				using System.Threading;
				using System.Threading.Tasks;

				class TestClassDeclaration {
					public void TestMethod() {
						int instance = 1;
					}

					public Task TestMethod(CancellationToken cancellationToken) {
						return Task.CompletedTask;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						var instance = new TestClassDeclaration();
						instance.TestMethod(CancellationToken.None);
					}
				}
			";

            await VerifyDiagnosticResults(code);
        }

        [Test]
        public async Task AG0021_Static_WhenInstanceAsyncWithPostfixExists_ShouldNotShowAnyWarning()
        {
            var code = @"
				using System.Threading.Tasks;

				class TestClassDeclaration {
					public static void TestMethod() {
						int instance = 1;
					}

					public Task TestMethodAsync() {
						return Task.CompletedTask;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						TestClassDeclaration.TestMethod();
					}
				}
			";

            await VerifyDiagnosticResults(code);
        }

        [Test]
        public async Task AG0021_Static_WhenInstanceAsyncWithoutPostfixExists_ShouldNotShowAnyWarning()
        {
            var code = @"
				using System.Threading;
				using System.Threading.Tasks;

				class TestClassDeclaration {
					public static void TestMethod() {
						int instance = 1;
					}

					public Task TestMethod(CancellationToken cancellationToken) {
						return Task.CompletedTask;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						TestClassDeclaration.TestMethod();
					}
				}
			";

            await VerifyDiagnosticResults(code);
        }

        [Test]
        public async Task AG0021_Instance_WhenStaticAsyncWithPostfixExists_ShouldNotShowAnyWarning()
        {
            var code = @"
				using System.Threading.Tasks;

				class TestClassDeclaration {
					public void TestMethod() {
						int instance = 1;
					}

					public static Task TestMethodAsync() {
						return Task.CompletedTask;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						var instance = new TestClassDeclaration();
						instance.TestMethod();
					}
				}
			";

            await VerifyDiagnosticResults(code);
        }

        [Test]
        public async Task AG0021_Instance_WhenStaticAsyncWithoutPostfixExists_ShouldNotShowAnyWarning()
        {
            var code = @"
				using System.Threading;
				using System.Threading.Tasks;

				class TestClassDeclaration {
					public void TestMethod() {
						int instance = 1;
					}

					public static Task TestMethod(CancellationToken cancellationToken) {
						return Task.CompletedTask;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						var instance = new TestClassDeclaration();
						instance.TestMethod();
					}
				}
			";

            await VerifyDiagnosticResults(code);
        }

        [Test]
        public async Task AG0021_Instance_WhenMethodIsInReferencedAssembly_ShouldShowWarning()
        {
            var code = @"
				class TestClassInvocation {
					public void InvocationMethod() {
						var stream = new System.IO.MemoryStream();
						stream.Flush(); // FlushAsync exists
					}
				}
			";

	        await VerifyDiagnosticResults(code, new DiagnosticLocation(5, 7));
        }

        [Test]
        public async Task AG0021_Instance_WhenAsyncWithoutPrefixIsExtended_ShouldNotShowWarning()
        {
            var code = @"
				using System.Threading;
				using System.Threading.Tasks;

				class TestClassDeclaration {
					public void TestMethod() {
						int instance = 1;
					}
				}

				static class TestClassExtensions {
					public static Task TestMethod(this TestClassDeclaration declaration, CancellationToken cancellationToken) {
						return Task.CompletedTask;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						var instance = new TestClassDeclaration();
						instance.TestMethod();
					}
				}
			";
	        
	        await VerifyDiagnosticResults(code, new DiagnosticLocation(20, 7));
        }

        [Test]
        public async Task AG0021_Instance_WhenAsyncWithPrefixIsExtended_ShouldNotShowWarning()
        {
            var code = @"
				using System.Threading;
				using System.Threading.Tasks;

				class TestClassDeclaration {
					public void TestMethod() {
						int instance = 1;
					}
				}

				static class TestClassExtensions {
					public static Task TestMethodAsync(this TestClassDeclaration declaration, CancellationToken cancellationToken) {
						return Task.CompletedTask;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						var instance = new TestClassDeclaration();
						instance.TestMethod();
					}
				}
			";

	        await VerifyDiagnosticResults(code, new DiagnosticLocation(20, 7));
        }

        [Test]
        public async Task AG0021_Instance_WhenUsedAndAsyncWithPrefixIsExtended_ShouldShowWarning()
        {
            var code = @"
				using System.Threading;
				using System.Threading.Tasks;

				class TestClassDeclaration {
				}

				static class TestClassExtensions {
					public static void TestMethod(this TestClassDeclaration declaration) {
						int instance = 1;
					}

					public static Task TestMethodAsync(this TestClassDeclaration declaration, CancellationToken cancellationToken) {
						return Task.CompletedTask;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						var instance = new TestClassDeclaration();
						instance.TestMethod();
					}
				}
			";

	        await VerifyDiagnosticResults(code, new DiagnosticLocation(21, 7));
        }

        [Test]
        public async Task AG0021_Instance_WhenUsedAndAsyncWithoutPrefixIsExtended_ShouldShowWarning()
        {
            var code = @"
				using System.Threading;
				using System.Threading.Tasks;

				class TestClassDeclaration {
				}

				static class TestClassExtensions {
					public static void TestMethod(this TestClassDeclaration declaration) {
						int instance = 1;
					}

					public static Task TestMethod(this TestClassDeclaration declaration, CancellationToken cancellationToken) {
						return Task.CompletedTask;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						var instance = new TestClassDeclaration();
						instance.TestMethod();
					}
				}
			";

	        await VerifyDiagnosticResults(code, new DiagnosticLocation(21, 7));
        }

        [Test]
        public async Task AG0021_Static_WhenUsedAndAsyncWithPrefixIsExtended_ShouldShowWarning()
        {
            var code = @"
				using System.Threading;
				using System.Threading.Tasks;

				class TestClassDeclaration {
				}

				static class TestClassExtensions {
					public static void TestMethod(this TestClassDeclaration declaration) {
						int instance = 1;
					}

					public static Task TestMethodAsync(this TestClassDeclaration declaration, CancellationToken cancellationToken) {
						return Task.CompletedTask;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						var instance = new TestClassDeclaration();
						TestClassExtensions.TestMethod(instance);
					}
				}
			";

	        await VerifyDiagnosticResults(code, new DiagnosticLocation(21, 7));
        }

        [Test]
        public async Task AG0021_Static_WhenUsedAndAsyncWithoutPrefixIsExtended_ShouldShowWarning()
        {
            var code = @"
				using System.Threading;
				using System.Threading.Tasks;

				class TestClassDeclaration {
				}

				static class TestClassExtensions {
					public static void TestMethod(this TestClassDeclaration declaration) {
						int instance = 1;
					}

					public static Task TestMethod(this TestClassDeclaration declaration, CancellationToken cancellationToken) {
						return Task.CompletedTask;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						var instance = new TestClassDeclaration();
						TestClassExtensions.TestMethod(instance);
					}
				}
			";

	        await VerifyDiagnosticResults(code, new DiagnosticLocation(21, 7));
        }
        
        [Test]
        public async Task AG0021_Instance_WhenUsedIsExtendedAndAsyncWithoutPrefixIsNot_ShouldShowWarning()
        {
            var code = @"
				using System.Threading;
				using System.Threading.Tasks;

				class TestClassDeclaration {
					public Task TestMethod(CancellationToken cancellationToken) {
						return Task.CompletedTask;
					}
				}

				static class TestClassExtensions {
					public static void TestMethod(this TestClassDeclaration declaration) {
						int instance = 1;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						var instance = new TestClassDeclaration();
						instance.TestMethod();
					}
				}
			";

	        await VerifyDiagnosticResults(code, new DiagnosticLocation(20, 7));
        }

        [Test]
        public async Task AG0021_Instance_WhenUsedIsExtendedAndAsyncWithPrefixIsNot_ShouldShowWarning()
        {
            var code = @"
				using System.Threading;
				using System.Threading.Tasks;

				class TestClassDeclaration {
					public Task TestMethodAsync(CancellationToken cancellationToken) {
						return Task.CompletedTask;
					}
				}

				static class TestClassExtensions {
					public static void TestMethod(this TestClassDeclaration declaration) {
						int instance = 1;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						var instance = new TestClassDeclaration();
						instance.TestMethod();
					}
				}
			";

	        await VerifyDiagnosticResults(code, new DiagnosticLocation(20, 7));
        }

        [Test]
        public async Task AG0021_Static_WhenUsedIsExtendedAndAsyncWithoutPrefixIsNot_ShouldShowWarning()
        {
            var code = @"
				using System.Threading;
				using System.Threading.Tasks;

				class TestClassDeclaration {
					public Task TestMethod(CancellationToken cancellationToken) {
						return Task.CompletedTask;
					}
				}

				static class TestClassExtensions {
					public static void TestMethod(this TestClassDeclaration declaration) {
						int instance = 1;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						var instance = new TestClassDeclaration();
						TestClassExtensions.TestMethod(instance);
					}
				}
			";
	        
	        await VerifyDiagnosticResults(code, new DiagnosticLocation(20, 7));
        }

        [Test]
        public async Task AG0021_Static_WhenUsedIsExtendedAndAsyncWithPrefixIsNot_ShouldShowWarning()
        {
            var code = @"
				using System.Threading;
				using System.Threading.Tasks;

				class TestClassDeclaration {
					public Task TestMethodAsync(CancellationToken cancellationToken) {
						return Task.CompletedTask;
					}
				}

				static class TestClassExtensions {
					public static void TestMethod(this TestClassDeclaration declaration) {
						int instance = 1;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						var instance = new TestClassDeclaration();
						TestClassExtensions.TestMethod(instance);
					}
				}
			";

	        await VerifyDiagnosticResults(code, new DiagnosticLocation(20, 7));
        }

        [Test]
        public async Task AG0021_Static_WhenAsyncMethodWithoutTaskExists_ShouldShowWarning()
        {
            var code = @"
				using System.Threading;
				using System.Threading.Tasks;
				using System.Runtime.CompilerServices;

				class TestClassDeclaration
				{
					public TaskAwaiter GetAwaiter()
					{
						return new TaskAwaiter();
					}

					public static void Method()
					{ }

					public static TestClassDeclaration MethodAsync()
					{
						return new TestClassDeclaration();
					}
				}

				class TestClassInvocation {
					private static void Test()
					{
						TestClassDeclaration.Method();
					}
				}
			";

	        await VerifyDiagnosticResults(code, new DiagnosticLocation(25, 7));
        }

        [Test]
        public async Task AG0021_Static_WhenUsedAsyncMethodWithoutTask_ShouldNotShowWarning()
        {
            var code = @"
				using System.Threading;
				using System.Threading.Tasks;
				using System.Runtime.CompilerServices;

				class TestClassDeclaration
				{
					public TaskAwaiter GetAwaiter()
					{
						return new TaskAwaiter();
					}

					public static void Method()
					{ }

					public static TestClassDeclaration MethodAsync()
					{
						return new TestClassDeclaration();
					}
				}

				class TestClassInvocation {
					private static async void Test()
					{
						await TestClassDeclaration.MethodAsync();
					}
				}
			";

            await VerifyDiagnosticResults(code);
        }

        [Test]
        public async Task AG0021_Instance_WhenAsyncWithoutPrefixIsInParent_ShouldShowWarning()
        {
            var code = @"
				using System.Threading;
				using System.Threading.Tasks;

				class TestClassDeclarationBase {
					public Task TestMethod(CancellationToken cancellationToken) {
						return Task.CompletedTask;
					}
				}

				class TestClassDeclaration: TestClassDeclarationBase {
					public void TestMethod() {
						int instance = 1;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						var instance = new TestClassDeclaration();
						instance.TestMethod();
					}
				}
			";

	        await VerifyDiagnosticResults(code, new DiagnosticLocation(20, 7));
        }

        [Test]
        public async Task AG0021_Instance_WhenAsyncWithoutPrefixIsInChild_ShouldShowWarning()
        {
            var code = @"
				using System.Threading;
				using System.Threading.Tasks;

				class TestClassDeclarationBase {
					public void TestMethod() {
						int instance = 1;
					}
				}

				class TestClassDeclaration: TestClassDeclarationBase {
					public Task TestMethod(CancellationToken cancellationToken) {
						return Task.CompletedTask;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						var instance = new TestClassDeclaration();
						instance.TestMethod();
					}
				}
			";

	        await VerifyDiagnosticResults(code, new DiagnosticLocation(20, 7));
        }

        [Test]
        public async Task AG0021_Instance_WhenAsyncWithPrefixIsInParent_ShouldShowWarning()
        {
            var code = @"
				using System.Threading;
				using System.Threading.Tasks;

				class TestClassDeclarationBase {
					public Task TestMethodAsync(CancellationToken cancellationToken) {
						return Task.CompletedTask;
					}
				}

				class TestClassDeclaration: TestClassDeclarationBase {
					public void TestMethod() {
						int instance = 1;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						var instance = new TestClassDeclaration();
						instance.TestMethod();
					}
				}
			";
	        
	        await VerifyDiagnosticResults(code, new DiagnosticLocation(20, 7));
        }

        [Test]
        public async Task AG0021_Instance_WhenAsyncWithPrefixIsInChild_ShouldShowWarning()
        {
            var code = @"
				using System.Threading;
				using System.Threading.Tasks;

				class TestClassDeclarationBase {
					public void TestMethod() {
						int instance = 1;
					}
				}

				class TestClassDeclaration: TestClassDeclarationBase {
					public Task TestMethodAsync(CancellationToken cancellationToken) {
						return Task.CompletedTask;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						var instance = new TestClassDeclaration();
						instance.TestMethod();
					}
				}
			";

	        await VerifyDiagnosticResults(code, new DiagnosticLocation(20, 7));
        }

        [Test]
        public async Task AG0021_Instance_WhenUsedDomesticMethod_ShouldShowWarning()
        {
            var code = @"
				using System.Threading;
				using System.Threading.Tasks;

				class TestClass {
					public void TestMethod() {
						int instance = 1;
					}

					public Task TestMethodAsync(CancellationToken cancellationToken) {
						return Task.CompletedTask;
					}

					public void InvocationMethod() {
						TestMethod();
					}
				}
			";

            await VerifyDiagnosticResults(code, new DiagnosticLocation(15, 7));
        }

        [Test]
        public async Task AG0021_Static_WhenUsedDomesticMethod_ShouldShowWarning()
        {
            var code = @"
				using System.Threading;
				using System.Threading.Tasks;

				class TestClass {
					public static void TestMethod() {
						int instance = 1;
					}

					public static Task TestMethodAsync(CancellationToken cancellationToken) {
						return Task.CompletedTask;
					}

					public static void InvocationMethod() {
						TestMethod();
					}
				}
			";

            await VerifyDiagnosticResults(code, new DiagnosticLocation(15, 7));
        }

        [Test]
        public async Task AG0021_Instance_WhenUsedFromFunctionResult_ShouldShowWarning()
        {
            var code = @"
				using System.Threading;
				using System.Threading.Tasks;

				class TestClassDeclaration {
					public void TestMethod() {
						int instance = 1;
					}

					public Task TestMethodAsync(CancellationToken cancellationToken) {
						return Task.CompletedTask;
					}

					public static TestClassDeclaration Create() {
						return new TestClassDeclaration();
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						TestClassDeclaration.Create().TestMethod();
					}
				}
			";

            await VerifyDiagnosticResults(code, new DiagnosticLocation(21, 7));
        }

        [Test]
        public async Task AG0021_Static_WhenUsedIsLocal_ShouldNotShowWarning()
        {
            var code = @"
				using System;
				using System.Threading;
				using System.Threading.Tasks;

				class TestClass {
					public Task TestMethodAsync(CancellationToken cancellationToken) {
						return Task.CompletedTask;
					}

					public void InvocationMethod() {
						Action TestMethod = () => {};

						TestMethod();
					}
				}
			";

            await VerifyDiagnosticResults(code);
        }

        [Test]
        public async Task AG0021_Instance_WhenAsyncIsNotAccessible_ShouldNotShowWarning()
        {
            var code = @"
				using System;
				using System.Threading;
				using System.Threading.Tasks;

				class TestClassDeclaration {
					public void TestMethod() {
						int instance = 1;
					}

					protected Task TestMethodAsync() {
						return Task.CompletedTask;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						var instance = new TestClassDeclaration();

						instance.TestMethod();
					}
				}
			";

            await VerifyDiagnosticResults(code);
        }

        [Test]
        public async Task AG0021_Static_WhenAsyncIsNotAccessible_ShouldNotShowWarning()
        {
            var code = @"
				using System;
				using System.Threading;
				using System.Threading.Tasks;

				class TestClassDeclaration {
					public static void TestMethod() {
						int instance = 1;
					}

					protected static Task TestMethodAsync() {
						return Task.CompletedTask;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						TestClassDeclaration.TestMethod();
					}
				}
			";

            await VerifyDiagnosticResults(code);
        }

        [Test]
        public async Task AG0021_Instance_WhenUsedOnClassField_ShouldShowWarning()
        {
            var code = @"
				using System;
				using System.Threading;
				using System.Threading.Tasks;

				class TestClassDeclaration {
					public void TestMethod() {
						int instance = 1;
					}

					public Task TestMethodAsync() {
						return Task.CompletedTask;
					}
				}

				class TestClassInvocation {
					public TestClassDeclaration Instance = new TestClassDeclaration();

					public void InvocationMethod() {
						Instance.TestMethod();
					}
				}
			";

            await VerifyDiagnosticResults(code, new DiagnosticLocation(20, 7));
        }

        [Test]
        public async Task AG0021_Instance_WhenUsedOnDynamic_ShouldNotShowWarning()
        {
            var code = @"
				using System;
				using System.Threading;
				using System.Threading.Tasks;

				class TestClassDeclaration {
					public void TestMethod() {
						int instance = 1;
					}

					public Task TestMethodAsync() {
						return Task.CompletedTask;
					}
				}

				class TestClassInvocation {
					public void InvocationMethod() {
						dynamic instance = new TestClassDeclaration();

						instance.TestMethod();
					}
				}
			";

            await VerifyDiagnosticResults(code);
        }

    }
}
