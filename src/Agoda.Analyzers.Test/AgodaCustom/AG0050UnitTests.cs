using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom;

class AG0050UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0050GeneratedClientModelInControllerAnalyzer();
    protected override string DiagnosticId => AG0050GeneratedClientModelInControllerAnalyzer.DIAGNOSTIC_ID;

    #region Positive Cases - Should Trigger Rule

    [Test]
    public async Task AG0050_DirectReturnType_HttpClient_ShouldShowWarning()
    {
        var code = @"
                using Microsoft.AspNetCore.Mvc;
                using UserService.Client;

                namespace Microsoft.AspNetCore.Mvc
                {
                    public class Controller { }
                    public class ActionResult<T> { }
                }

                namespace Controllers
                {
                    public class TestController : Controller
                    {
                        public ActionResult<UserDto> GetUser(int id)
                        {
                            return new ActionResult<UserDto>();
                        }
                    }
                }
                
                namespace UserService.Client
                {
                    public class UserDto 
                    {
                        public string Name { get; set; }
                    }
                }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(15, 32, "GetUser", "UserDto", "TestProject"));
    }

    [Test]
    public async Task AG0050_DirectReturnType_GraphqlClient_ShouldShowWarning()
    {
        var code = @"
                using Microsoft.AspNetCore.Mvc;
                using Orders.Graphql;

                namespace Microsoft.AspNetCore.Mvc
                {
                    public class Controller { }
                    public class ActionResult<T> { }
                }

                namespace Controllers
                {
                    public class TestController : Controller
                    {
                        public ActionResult<OrderModel> GetOrder(int id)
                        {
                            return new ActionResult<OrderModel>();
                        }
                    }
                }
                
                namespace Orders.Graphql
                {
                    public class OrderModel 
                    {
                        public decimal Amount { get; set; }
                    }
                }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(15, 32, "GetOrder", "OrderModel", "TestProject"));
    }

    [Test]
    public async Task AG0050_AsyncReturnType_ShouldShowWarning()
    {
        var code = @"
                using System.Threading.Tasks;
                using Microsoft.AspNetCore.Mvc;
                using UserService.Client;

                namespace Microsoft.AspNetCore.Mvc
                {
                    public class Controller { }
                    public class ActionResult<T> { }
                }

                namespace Controllers
                {
                    public class TestController : Controller
                    {
                        public async Task<ActionResult<UserDto>> GetUserAsync(int id)
                        {
                            return new ActionResult<UserDto>();
                        }
                    }
                }
                
                namespace UserService.Client
                {
                    public class UserDto 
                    {
                        public string Name { get; set; }
                    }
                }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(16, 38, "GetUserAsync", "UserDto", "TestProject"));
    }

    [Test]
    public async Task AG0050_CollectionReturnType_ShouldShowWarning()
    {
        var code = @"
                using System.Collections.Generic;
                using Microsoft.AspNetCore.Mvc;
                using Products.Client;

                namespace Microsoft.AspNetCore.Mvc
                {
                    public class Controller { }
                    public class ActionResult<T> { }
                }

                namespace Controllers
                {
                    public class TestController : Controller
                    {
                        public ActionResult<List<ProductModel>> GetProducts()
                        {
                            return new ActionResult<List<ProductModel>>();
                        }
                    }
                }
                
                namespace Products.Client
                {
                    public class ProductModel 
                    {
                        public string Name { get; set; }
                    }
                }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(16, 32, "GetProducts", "ProductModel", "TestProject"));
    }

    [Test]
    public async Task AG0050_GeneratedModelInResponseProperty_ShouldShowWarning()
    {
        var code = @"
                using Microsoft.AspNetCore.Mvc;
                using UserService.Client;

                namespace Microsoft.AspNetCore.Mvc
                {
                    public class Controller { }
                    public class ActionResult<T> { }
                }

                namespace Controllers
                {
                    public class TestController : Controller
                    {
                        public ActionResult<UserResponse> GetUser(int id)
                        {
                            return new ActionResult<UserResponse>();
                        }
                    }

                    public class UserResponse
                    {
                        public UserDto User { get; set; } // Generated client model property
                        public string AdditionalInfo { get; set; }
                    }
                }
                
                namespace UserService.Client
                {
                    public class UserDto 
                    {
                        public string Name { get; set; }
                    }
                }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(15, 32, "GetUser", "UserDto", "TestProject"));
    }

    [Test]
    public async Task AG0050_GeneratedModelInInputParameter_Direct_ShouldShowWarning()
    {
        var code = @"
                using Microsoft.AspNetCore.Mvc;
                using UserService.Client;

                namespace Microsoft.AspNetCore.Mvc
                {
                    public class Controller { }
                    public class ActionResult<T> { }
                }

                namespace Controllers
                {
                    public class TestController : Controller
                    {
                        public ActionResult<UserResponse> UpdateUser(UserDto userInput)
                        {
                            return new ActionResult<UserResponse>();
                        }
                    }

                    public class UserResponse
                    {
                        public string Name { get; set; }
                    }
                }
                
                namespace UserService.Client
                {
                    public class UserDto 
                    {
                        public string Name { get; set; }
                    }
                }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(15, 70, "UpdateUser", "UserDto", "TestProject"));
    }

    [Test]
    public async Task AG0050_GeneratedModelInInputParameter_Nested_ShouldShowWarning()
    {
        var code = @"
                using Microsoft.AspNetCore.Mvc;
                using UserService.Client;

                namespace Microsoft.AspNetCore.Mvc
                {
                    public class Controller { }
                    public class ActionResult<T> { }
                }

                namespace Controllers
                {
                    public class TestController : Controller
                    {
                        public ActionResult<UserResponse> UpdateUser(UpdateRequest request)
                        {
                            return new ActionResult<UserResponse>();
                        }
                    }

                    public class UpdateRequest
                    {
                        public UserDto UserData { get; set; } // Generated client model in parameter property
                        public string Reason { get; set; }
                    }

                    public class UserResponse
                    {
                        public string Name { get; set; }
                    }
                }
                
                namespace UserService.Client
                {
                    public class UserDto 
                    {
                        public string Name { get; set; }
                    }
                }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(15, 70, "UpdateUser", "UserDto", "TestProject"));
    }

    [Test]
    public async Task AG0050_ControllerBase_ShouldShowWarning()
    {
        var code = @"
                using Microsoft.AspNetCore.Mvc;
                using UserService.Client;

                namespace Microsoft.AspNetCore.Mvc
                {
                    public class ControllerBase { }
                    public class ActionResult<T> { }
                }

                namespace Controllers
                {
                    public class TestController : ControllerBase
                    {
                        public ActionResult<UserDto> GetUser(int id)
                        {
                            return new ActionResult<UserDto>();
                        }
                    }
                }
                
                namespace UserService.Client
                {
                    public class UserDto 
                    {
                        public string Name { get; set; }
                    }
                }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(15, 32, "GetUser", "UserDto", "TestProject"));
    }

    #endregion

    #region Negative Cases - Should Not Trigger Rule

    [Test]
    public async Task AG0050_DedicatedResponseModels_ShouldNotShowWarning()
    {
        var code = @"
                using Microsoft.AspNetCore.Mvc;

                namespace Microsoft.AspNetCore.Mvc
                {
                    public class Controller { }
                    public class ActionResult<T> { }
                }

                namespace Controllers
                {
                    public class TestController : Controller
                    {
                        public ActionResult<UserResponse> GetUser(int id)
                        {
                            return new ActionResult<UserResponse>();
                        }

                        public ActionResult<UserResponse> UpdateUser(UpdateUserRequest request)
                        {
                            return new ActionResult<UserResponse>();
                        }
                    }

                    public class UserResponse 
                    {
                        public string Name { get; set; }
                        public int Age { get; set; }
                    }

                    public class UpdateUserRequest 
                    {
                        public string Name { get; set; }
                        public int Age { get; set; }
                    }
                }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation[0]);
    }

    [Test]
    public async Task AG0050_NonControllerClass_ShouldNotShowWarning()
    {
        var code = @"
                using UserService.Client;

                namespace Services
                {
                    public class UserService 
                    {
                        public UserDto GetUser()
                        { 
                            return new UserDto();
                        }

                        private void ProcessUser(UserDto user)
                        { 
                        }
                    }
                }
                
                namespace UserService.Client
                {
                    public class UserDto 
                    {
                        public string Name { get; set; }
                    }
                }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation[0]);
    }

    [Test]
    public async Task AG0050_SimilarNamingButDifferentAssembly_ShouldNotShowWarning()
    {
        var code = @"
                using Microsoft.AspNetCore.Mvc;
                using MyApp.Services;

                namespace Microsoft.AspNetCore.Mvc
                {
                    public class Controller { }
                    public class ActionResult<T> { }
                }

                namespace Controllers
                {
                    public class TestController : Controller
                    {
                        public ActionResult<UserResponse> GetUser(int id)
                        {
                            return new ActionResult<UserResponse>();
                        }
                    }

                    public class UserResponse 
                    {
                        public UserDto User { get; set; } // OK - not from generated assembly
                    }
                }
                
                namespace MyApp.Services
                {
                    public class UserDto 
                    {
                        public string Name { get; set; }
                    }
                }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation[0]);
    }

    [Test]
    public async Task AG0050_PrivateControllerMethods_ShouldNotShowWarning()
    {
        var code = @"
                using Microsoft.AspNetCore.Mvc;
                using UserService.Client;

                namespace Microsoft.AspNetCore.Mvc
                {
                    public class Controller { }
                    public class ActionResult<T> { }
                }

                namespace Controllers
                {
                    public class TestController : Controller
                    {
                        public ActionResult<UserResponse> GetUser(int id)
                        {
                            var userData = GetUserFromService();
                            return new ActionResult<UserResponse>();
                        }

                        private UserDto GetUserFromService()
                        {
                            return new UserDto();
                        }
                    }

                    public class UserResponse 
                    {
                        public string Name { get; set; }
                    }
                }
                
                namespace UserService.Client
                {
                    public class UserDto 
                    {
                        public string Name { get; set; }
                    }
                }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation[0]);
    }

    [Test]
    public async Task AG0050_NonActionMethods_ShouldNotShowWarning()
    {
        var code = @"
                using Microsoft.AspNetCore.Mvc;
                using UserService.Client;

                namespace Microsoft.AspNetCore.Mvc
                {
                    public class Controller { }
                    public class ActionResult<T> { }
                    public class NonActionAttribute : System.Attribute { }
                }

                namespace Controllers
                {
                    public class TestController : Controller
                    {
                        [NonAction]
                        public UserDto GetUserData()
                        {
                            return new UserDto();
                        }

                        protected UserDto GetProtectedUserData()
                        {
                            return new UserDto();
                        }

                        internal UserDto GetInternalUserData()
                        {
                            return new UserDto();
                        }
                    }
                }
                
                namespace UserService.Client
                {
                    public class UserDto 
                    {
                        public string Name { get; set; }
                    }
                }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation[0]);
    }

    #endregion

    #region Edge Cases

    [Test]
    public async Task AG0050_CircularReference_ShouldHandleWithoutInfiniteLoop()
    {
        var code = @"
                using Microsoft.AspNetCore.Mvc;
                using UserService.Client;

                namespace Microsoft.AspNetCore.Mvc
                {
                    public class Controller { }
                    public class ActionResult<T> { }
                }

                namespace Controllers
                {
                    public class TestController : Controller
                    {
                        public ActionResult<CircularReference> GetData()
                        {
                            return new ActionResult<CircularReference>();
                        }
                    }

                    public class CircularReference
                    {
                        public CircularReference Child { get; set; }
                        public UserDto User { get; set; } // Should still detect this
                    }
                }
                
                namespace UserService.Client
                {
                    public class UserDto 
                    {
                        public string Name { get; set; }
                    }
                }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(15, 32, "GetData", "UserDto", "TestProject"));
    }

    [Test]
    public async Task AG0050_NullableTypes_ShouldShowWarning()
    {
        var code = @"
                using Microsoft.AspNetCore.Mvc;
                using UserService.Client;

                namespace Microsoft.AspNetCore.Mvc
                {
                    public class Controller { }
                    public class ActionResult<T> { }
                }

                namespace Controllers
                {
                    public class TestController : Controller
                    {
                        public ActionResult<UserDto?> GetUser(int id)
                        {
                            return new ActionResult<UserDto?>();
                        }
                    }
                }
                
                namespace UserService.Client
                {
                    public class UserDto 
                    {
                        public string Name { get; set; }
                    }
                }";

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(15, 32, "GetUser", "UserDto", "TestProject"));
    }

    #endregion
} 