using System.Collections.Immutable;
using System.Resources;
using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Agoda.Analyzers.AgodaCustom
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0039MethodLineLengthAnalyzer : DiagnosticAnalyzer
    {
        private const int MAX_LINE_DEFAULT = 40; // Adjust the maximum allowed lines as needed.
        public const string DIAGNOSTIC_ID = "AG0039";
        private int MaxLines = MAX_LINE_DEFAULT;
        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0039Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(CustomRulesResources.AG0039Title),
            CustomRulesResources.ResourceManager,
            typeof(CustomRulesResources));

        private static readonly LocalizableString Description
            = DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0039MethodLineLengthAnalyzer));

        private const string Category = "Documentation";
        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DIAGNOSTIC_ID,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Hidden, // THis rule should be opt in and not on by default
            isEnabledByDefault: true,
            helpLinkUri: "https://github.com/agoda-com/AgodaAnalyzers/blob/master/src/Agoda.Analyzers/RuleContent/AG0039MethodLineLengthAnalyzer.html");
        
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Descriptor); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(compilationContext =>
            {
                var optionsProvider = compilationContext.Options.AnalyzerConfigOptionsProvider;
                var options = optionsProvider.GetOptions(compilationContext.Compilation.SyntaxTrees.First());

                if (options.TryGetValue("dotnet_diagnostic.AG0039.method_line_length_limit", out var configValue))
                {
                    if (int.TryParse(configValue, out var i))
                    {
                        MaxLines = i;
                    }
                }

                compilationContext.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
            });
        }

        private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var methodNode = (MethodDeclarationSyntax)context.Node;
            var lineCount = GetNonEmptyLineCount(methodNode.GetText().Lines);

            if (lineCount <= MaxLines) return;

            var diagnostic = Diagnostic.Create(Descriptor, methodNode.Identifier.GetLocation(), properties: _props.ToImmutableDictionary(), methodNode.Identifier.Text, lineCount, MaxLines);
            context.ReportDiagnostic(diagnostic);
        }

        private static int GetNonEmptyLineCount(IEnumerable<TextLine> lines)
        {
            return lines.Count(line => !string.IsNullOrWhiteSpace(line.ToString()));
        }

        private static Dictionary<string, string> _props = new Dictionary<string, string>()
        {
            { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "10" }
        };
    }
}