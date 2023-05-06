using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Agoda.Analyzers.Test.Helpers;

/// <summary>
/// Metadata references used to create test projects.
/// </summary>
internal static class MetadataReferences
{
    internal static readonly MetadataReference CorlibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location).WithAliases(ImmutableArray.Create("global", "corlib"));
    internal static readonly MetadataReference SystemReference = MetadataReference.CreateFromFile(typeof(Debug).Assembly.Location).WithAliases(ImmutableArray.Create("global", "system"));
    internal static readonly MetadataReference SystemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
    internal static readonly MetadataReference CSharpSymbolsReference = MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);
    internal static readonly MetadataReference CodeAnalysisReference = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);
}