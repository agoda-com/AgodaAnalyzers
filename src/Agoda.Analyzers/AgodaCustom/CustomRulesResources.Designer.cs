﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Agoda.Analyzers.AgodaCustom {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class CustomRulesResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal CustomRulesResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Agoda.Analyzers.AgodaCustom.CustomRulesResources", typeof(CustomRulesResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Access dependencies in a resolver-agnostic way.
        /// </summary>
        public static string AG0001Description {
            get {
                return ResourceManager.GetString("AG0001Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Access dependencies in a resolver-agnostic way.
        /// </summary>
        public static string AG0001MessageFormat {
            get {
                return ResourceManager.GetString("AG0001MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do not use DependencyResolver directly.
        /// </summary>
        public static string AG0001Title {
            get {
                return ResourceManager.GetString("AG0001Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Test only the public surface of your API.
        /// </summary>
        public static string AG0002Description {
            get {
                return ResourceManager.GetString("AG0002Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Test only the public surface of your API.
        /// </summary>
        public static string AG0002MessageFormat {
            get {
                return ResourceManager.GetString("AG0002MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Not test private methods.
        /// </summary>
        public static string AG0002Title {
            get {
                return ResourceManager.GetString("AG0002Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Pass only the fields that are actually needed, not the entire HttpContext instance.
        /// </summary>
        public static string AG0003Description {
            get {
                return ResourceManager.GetString("AG0003Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Pass only the fields that are actually needed, not the entire HttpContext instance.
        /// </summary>
        public static string AG0003MessageFormat {
            get {
                return ResourceManager.GetString("AG0003MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do not pass HttpContext as method argument.
        /// </summary>
        public static string AG0003Title {
            get {
                return ResourceManager.GetString("AG0003Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do not use hard coded string to identify types.
        /// </summary>
        public static string AG0004Title {
            get {
                return ResourceManager.GetString("AG0004Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Test method names must follow convention.
        /// </summary>
        public static string AG0005Title {
            get {
                return ResourceManager.GetString("AG0005Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Container registered component must have only one public constructor.
        /// </summary>
        public static string AG0006Title {
            get {
                return ResourceManager.GetString("AG0006Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Pass only the fields that are actually needed, not the entire IHttpContextAccessor instance.
        /// </summary>
        public static string AG0009Description {
            get {
                return ResourceManager.GetString("AG0009Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Pass only the fields that are actually needed, not the entire IHttpContextAccessor instance.
        /// </summary>
        public static string AG0009MessageFormat {
            get {
                return ResourceManager.GetString("AG0009MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do not pass IHttpContextAccessor as method argument.
        /// </summary>
        public static string AG0009Title {
            get {
                return ResourceManager.GetString("AG0009Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Prevent test fixture inheritance.
        /// </summary>
        public static string AG0010Title {
            get {
                return ResourceManager.GetString("AG0010Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Controllers should not access HttpRequest.QueryString directly but use ASP.NET model binding instead.
        /// </summary>
        public static string AG0011Title {
            get {
                return ResourceManager.GetString("AG0011Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Test method should contain at least one assertion.
        /// </summary>
        public static string AG0012Title {
            get {
                return ResourceManager.GetString("AG0012Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Limit number of test method parameters to 5.
        /// </summary>
        public static string AG0013Title {
            get {
                return ResourceManager.GetString("AG0013Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Ensure that publicly exposed IEnumerable types.
        /// </summary>
        public static string AG0018Title {
            get {
                return ResourceManager.GetString("AG0018Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Remove InternalsVisibleTo attribute.
        /// </summary>
        public static string AG0019FixTitle {
            get {
                return ResourceManager.GetString("AG0019FixTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do not use System.Runtime.CompilerServices.InternalsVisibleTo attribute.
        /// </summary>
        public static string AG0019Title {
            get {
                return ResourceManager.GetString("AG0019Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Return an empty IEnumerable&lt;T&gt;.
        /// </summary>
        public static string AG0020FixTitle {
            get {
                return ResourceManager.GetString("AG0020FixTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do not return null when the return value is IEnumerable&lt;T&gt;.
        /// </summary>
        public static string AG0020Title {
            get {
                return ResourceManager.GetString("AG0020Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do not use synchronous version of method when async version exists.
        /// </summary>
        public static string AG0021Title {
            get {
                return ResourceManager.GetString("AG0021Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do not expose both sync and async versions of methods.
        /// </summary>
        public static string AG0022Description {
            get {
                return ResourceManager.GetString("AG0022Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Remove sync version of this method.
        /// </summary>
        public static string AG0022FixTitle {
            get {
                return ResourceManager.GetString("AG0022FixTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do not expose both sync and async versions of methods.
        /// </summary>
        public static string AG0022MessageFormat {
            get {
                return ResourceManager.GetString("AG0022MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do not expose both sync and async versions of methods.
        /// </summary>
        public static string AG0022Title {
            get {
                return ResourceManager.GetString("AG0022Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Prevent the use of Thread.Sleep.
        /// </summary>
        public static string AG0023Title {
            get {
                return ResourceManager.GetString("AG0023Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Prevent use of Task.Factory.StartNew except for Long Running Tasks.
        /// </summary>
        public static string AG0024Title {
            get {
                return ResourceManager.GetString("AG0024Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Prevent use of Task.Continue*.
        /// </summary>
        public static string AG0025Title {
            get {
                return ResourceManager.GetString("AG0025Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use only CSS Selectors to find elements in Selenium tests.
        /// </summary>
        public static string AG0026Title {
            get {
                return ResourceManager.GetString("AG0026Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Elements must be selected by a data-selenium HTML attribute in Selenium tests.
        /// </summary>
        public static string AG0027Title {
            get {
                return ResourceManager.GetString("AG0027Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Prevent use of dynamic.
        /// </summary>
        public static string AG0030Title {
            get {
                return ResourceManager.GetString("AG0030Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Prevent use of blocking Task.Wait*.
        /// </summary>
        public static string AG0032Title {
            get {
                return ResourceManager.GetString("AG0032Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Prevent use of blocking Task.Result.
        /// </summary>
        public static string AG0033Title {
            get {
                return ResourceManager.GetString("AG0033Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use of MachineName tightly couples your code to our infrastructure and its naming scheme, which can and will change over time.
        ///Your code should be agnostic of environment, data center, cluster and server. Having different code paths for different environments can lead to bugs that can only be caught in production.
        ///Such environmental variations are usually only required when calling external services, as you will want to call the service running in your local data center. For this, use Consul&apos;s service dis [rest of string was truncated]&quot;;.
        /// </summary>
        public static string AG0035Description {
            get {
                return ResourceManager.GetString("AG0035Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do not use MachineName.
        /// </summary>
        public static string AG0035Title {
            get {
                return ResourceManager.GetString("AG0035Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A Selenium test case - and/or its entire test class - must be decorated with the [OwnedBy()] attribute.
        /// </summary>
        public static string AG0037Description {
            get {
                return ResourceManager.GetString("AG0037Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A Selenium test must indicate the team responsible for its maintenance.
        /// </summary>
        public static string AG0037Title {
            get {
                return ResourceManager.GetString("AG0037Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do not use #region directives.
        /// </summary>
        public static string AG0038Title {
            get {
                return ResourceManager.GetString("AG0038Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Method {0} has {1} lines of code and is potentially not readable, consider refactoring for better readability {2} non-whitespace lines is what we recommend as a maximum, but its up to individual context.
        /// </summary>
        public static string AG0039Title {
            get {
                return ResourceManager.GetString("AG0039Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do not use WaitUntilState.NetworlIdle. It makes test flaky. Don&apos;t use this method for testing, rely on web assertions to assess readiness instead..
        /// </summary>
        public static string AG0040MessageFormat {
            get {
                return ResourceManager.GetString("AG0040MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do not use WaitUntilState.NetworlIdle. It makes test flaky. Don&apos;t use this method for testing, rely on web assertions to assess readiness instead..
        /// </summary>
        public static string AG0040Title {
            get {
                return ResourceManager.GetString("AG0040Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You are using either an interpolated string or string concatenation in your logs, change these to the message template format to preserve structure in your logs.
        /// </summary>
        public static string AG0041Title {
            get {
                return ResourceManager.GetString("AG0041Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ElementHandler methods should not be used. Use ILocator based approaches instead..
        /// </summary>
        public static string AG0042Title {
            get {
                return ResourceManager.GetString("AG0042Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to BuildServiceProvider detected in request processing code. This may cause memory leaks. Remove the BuildServiceProvider() call and let the framework manage the service provider lifecycle..
        /// </summary>
        public static string AG0043Title {
            get {
                return ResourceManager.GetString("AG0043Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Force options bypass Playwright&apos;s built-in checks and can lead to unstable tests. Use proper waiting and interaction patterns instead..
        /// </summary>
        public static string AG0044Description {
            get {
                return ResourceManager.GetString("AG0044Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Avoid using Force option in Locator methods as it bypasses important element state validations.
        /// </summary>
        public static string AG0044MessageFormat {
            get {
                return ResourceManager.GetString("AG0044MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Force option should not be used in Locator methods.
        /// </summary>
        public static string AG0044Title {
            get {
                return ResourceManager.GetString("AG0044Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to XPath selectors are brittle and tightly coupled to DOM structure. Use Playwright&apos;s built-in locators like GetByRole, GetByText, GetByTestId, etc. for more reliable tests..
        /// </summary>
        public static string AG0045Description {
            get {
                return ResourceManager.GetString("AG0045Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to XPath selectors create brittle tests. Use Playwright&apos;s recommended locators instead..
        /// </summary>
        public static string AG0045MessageFormat {
            get {
                return ResourceManager.GetString("AG0045MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to XPath selectors should not be used in Playwright locators.
        /// </summary>
        public static string AG0045Title {
            get {
                return ResourceManager.GetString("AG0045Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to When using TestContainers with a mirror/proxy pull-through cache for Docker, the TESTCONTAINERS_HUB_IMAGE_NAME_PREFIX environment variable must be set before calling Build() on any container. This ensures that all container images are pulled from the correct mirror..
        /// </summary>
        public static string AG0047Description {
            get {
                return ResourceManager.GetString("AG0047Description", resourceCulture);
                }
         }
         
        /// <summary>
        ///   Looks up a localized string similar to Using test IDs for element selection creates a clear contract between the UI and tests, leading to more stable and maintainable test automation. Other locator methods like GetByText(), GetByRole(), etc. can make tests brittle and tightly coupled to UI changes..
        /// </summary>
        public static string AG0046Description {
            get {
                return ResourceManager.GetString("AG0046Description", resourceCulture);
                }
         }
        /// <summary>
        ///   Looks up a localized string similar to Exception should be passed as the first parameter to logger methods, not as a message parameter.
        /// </summary>
        public static string AG0048MessageFormat {
            get {
                return ResourceManager.GetString("AG0048MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to TestContainer Build() must be called after setting {0} environment variable to ensure images are pulled from the correct mirror.
        /// </summary>
        public static string AG0047MessageFormat {
            get {
                return ResourceManager.GetString("AG0047MessageFormat", resourceCulture);
                }
         }
        /// <summary>
        ///   Looks up a localized string similar to Use GetByTestId() to create a stable testing contract instead of {0}.
        /// </summary>
        public static string AG0046MessageFormat {
            get {
                return ResourceManager.GetString("AG0046MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to TestContainer Build() must be called after setting TESTCONTAINERS_HUB_IMAGE_NAME_PREFIX environment variable.
        /// </summary>
        public static string AG0047Title {
            get {
                return ResourceManager.GetString("AG0047Title", resourceCulture);
                }
         }

        /// <summary>
        ///   Looks up a localized string similar to Use GetByTestId() instead of other locator methods.
        /// </summary>
        public static string AG0046Title {
            get {
                return ResourceManager.GetString("AG0046Title", resourceCulture);
            }
        }
        /// <summary>
        ///   Looks up a localized string similar to Do not pass exception as message parameter to logger methods.
        /// </summary>
        public static string AG0048Title {
            get {
                return ResourceManager.GetString("AG0048Title", resourceCulture);
            }
        }
    }
}
