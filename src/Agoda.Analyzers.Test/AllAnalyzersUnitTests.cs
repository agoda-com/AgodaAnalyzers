using System;
using System.Linq;
using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test
{
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
    }
}