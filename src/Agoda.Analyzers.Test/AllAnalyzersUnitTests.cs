using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    public void Analyzer_Should_Pass_Properties_To_Diagnostic_Create(Type analyzerType)
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
                foreach (var diagnosticCreateCall in FindDiagnosticCreateCalls(method))
                {
                    if (!diagnosticCreateCall.HasPropertiesParameter)
                    {
                        violations.Add($"Method {method.Name} in {analyzerType.Name} calls Diagnostic.Create without properties parameter");
                    }
                }
            }
            catch (Exception ex)
            {
                violations.Add($"Error analyzing method {method.Name}: {ex.Message}");
            }
        }

        violations.ShouldBeEmpty();
    }

    private class DiagnosticCreateCall
    {
        public bool HasPropertiesParameter { get; set; }
        public int ParameterCount { get; set; }
    }

    private IEnumerable<DiagnosticCreateCall> FindDiagnosticCreateCalls(MethodInfo method)
    {
        var calls = new List<DiagnosticCreateCall>();
        try
        {
            var body = method.GetMethodBody();
            if (body == null) return calls;

            var instructions = body.GetILAsByteArray();
            if (instructions == null) return calls;

            var moduleHandle = method.Module.ModuleHandle;

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
                            var parameters = calledMethod.GetParameters();
                            var hasPropertiesParam = parameters.Any(p =>
                                p.ParameterType.IsGenericType &&
                                p.ParameterType.GetGenericTypeDefinition() == typeof(ImmutableDictionary<,>) &&
                                p.ParameterType.GetGenericArguments()[0] == typeof(string) &&
                                p.ParameterType.GetGenericArguments()[1] == typeof(string));

                            calls.Add(new DiagnosticCreateCall
                            {
                                HasPropertiesParameter = hasPropertiesParam,
                                ParameterCount = parameters.Length
                            });
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
            // If we can't analyze the method, we'll skip it
        }

        return calls;
    }
}