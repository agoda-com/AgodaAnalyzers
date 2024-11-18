using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using Shouldly;

namespace Agoda.Analyzers.Test;

public class AllAnalyzersUnitTests
{
    /// <summary>
    /// Descriptor.Title is required by SonarQube.
    /// </summary>
    [Test]
    public void Analyzer_MustHaveDescriptorTitle()
    {
        var types = typeof(TestMethodHelpers).Assembly.GetTypes()
            .Where(t => typeof(DiagnosticAnalyzer).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        Assert.Multiple(() =>
        {
            foreach (var type in types)
            {
                var analyzer = (DiagnosticAnalyzer) Activator.CreateInstance(type);
                if (analyzer.SupportedDiagnostics.Any(d => string.IsNullOrEmpty(d.Title.ToString())))
                {
                    Assert.Fail($"Analyzer {type} must define Descriptor.Title");
                }
            }    
        });
    }
    private static IEnumerable<TestCaseData> GetAnalyzerTestCases()
    {
        var assembly = typeof(AnalyzerConstants).Assembly;

        return assembly.GetTypes()
            .Where(t => typeof(DiagnosticAnalyzer).IsAssignableFrom(t) && !t.IsAbstract)
            .Select(analyzerType => new TestCaseData(analyzerType)
                .SetName(analyzerType.Name)
                .SetDescription($"Verifies that {analyzerType.Name} has required properties for all Diagnostic.Create calls"));
    }

    [TestCaseSource(nameof(GetAnalyzerTestCases))]
    public void Analyzer_Should_Have_Required_Properties_For_Diagnostic_Create(Type analyzerType)
    {
        var methods = analyzerType.GetMethods(BindingFlags.Instance |
                                              BindingFlags.NonPublic |
                                              BindingFlags.Public |
                                              BindingFlags.DeclaredOnly |
                                              BindingFlags.Static |  
                                              BindingFlags.FlattenHierarchy); 

        var violations = new List<string>();

        foreach (var method in methods)
        {
            try
            {
                // Get the IL bytes of the method
                var methodBody = method.GetMethodBody();
                if (methodBody == null) continue;

                var instructions = methodBody.GetILAsByteArray();
                if (instructions == null) continue;

                // Check if the method contains a call to Diagnostic.Create
                if (ContainsDiagnosticCreate(method))
                {
                    // Create instance of analyzer to check properties
                    var analyzer = Activator.CreateInstance(analyzerType);

                    // Get the Properties dictionary/field
                    var propertiesInfo = analyzerType.GetProperty("Properties",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                    propertiesInfo.ShouldNotBeNull($"Analyzer uses Diagnostic.Create in method {method.Name} " +
                                                   "but doesn't have a Properties property");

                    var properties = propertiesInfo.GetValue(analyzer) as IDictionary<string, string>;

                    properties.ShouldNotBeNull($"Properties is null or not a dictionary");

                    properties.ContainsKey(AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES)
                        .ShouldBeTrue($"Method {method.Name} doesn't contain required '{AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES}' property When calling Diagnostic.Create");

                    properties["MyKey"].ShouldNotBeNullOrWhiteSpace(
                        $"MyKey property in method {method.Name} should have a value");
                }
            }
            catch (Exception ex)
            {
                violations.Add($"Error analyzing method {method.Name}: {ex.Message}");
            }
        }

        violations.ShouldBeEmpty();
    }

    private bool ContainsDiagnosticCreate(MethodInfo method)
    {
        try
        {
            // Get all method calls within the method
            var body = method.GetMethodBody();
            if (body == null) return false;

            // Decompile IL to find calls to Diagnostic.Create
            var instructions = body.GetILAsByteArray();
            if (instructions == null) return false;

            // Get all method calls from the IL
            var moduleHandle = method.Module.ModuleHandle;
            var methodCalls = new List<MethodInfo>();

            for (int i = 0; i < instructions.Length; i++)
            {
                // Look for call or callvirt instructions
                if (instructions[i] == 0x28 || instructions[i] == 0x6F) // call or callvirt
                {
                    var methodToken = BitConverter.ToInt32(instructions, i + 1);
                    try
                    {
                        var calledMethod = MethodBase.GetMethodFromHandle(
                            moduleHandle.ResolveMethodHandle(methodToken));

                        if (calledMethod?.DeclaringType?.FullName == "Microsoft.CodeAnalysis.Diagnostic" &&
                            calledMethod.Name == "Create")
                        {
                            return true;
                        }
                    }
                    catch
                    {
                        // Skip if we can't resolve the method
                        continue;
                    }
                }
            }
        }
        catch
        {
            // If we can't analyze the method, assume it might contain Diagnostic.Create
            return true;
        }

        return false;
    }
}