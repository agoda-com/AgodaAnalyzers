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
        private const int MaxLines = 40; // Adjust the maximum allowed lines as needed.

        public const string DIAGNOSTIC_ID = "AG0039";

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
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Method is longer than 120 lines of code, please consider refactoring to make it more readable.");
        
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Descriptor); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var methodNode = (MethodDeclarationSyntax)context.Node;
            var lineCount = GetNonEmptyLineCount(methodNode.GetText().Lines);

            if (lineCount <= MaxLines) return;

            var diagnostic = Diagnostic.Create(Descriptor, methodNode.Identifier.GetLocation(), methodNode.Identifier.Text, lineCount, MaxLines);
            context.ReportDiagnostic(diagnostic);
        }

        private static int GetNonEmptyLineCount(IEnumerable<TextLine> lines)
        {
            return lines.Count(line => !string.IsNullOrWhiteSpace(line.ToString()));
        }
    }
}