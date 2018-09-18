﻿using System;
using System.Collections.Immutable;
using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Agoda.Analyzers.StyleCop
{
    /// <summary>
    /// The C# code contains more than one statement on a single line.
    /// </summary>
    /// <remarks>
    /// <para>A violation of this rule occurs when the code contain more than one statement on the same line. Each
    /// statement must begin on a new line.</para>
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SA1107CodeMustNotContainMultipleStatementsOnOneLine : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="SA1107CodeMustNotContainMultipleStatementsOnOneLine"/>
        /// analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = "SA1107";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(ReadabilityResources.SA1107Title), ReadabilityResources.ResourceManager, typeof(ReadabilityResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(ReadabilityResources.SA1107MessageFormat), ReadabilityResources.ResourceManager, typeof(ReadabilityResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(ReadabilityResources.SA1107Description), ReadabilityResources.ResourceManager, typeof(ReadabilityResources));
        private static readonly string HelpLink = "https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1107.md";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DIAGNOSTIC_ID, Title, MessageFormat, AnalyzerCategory.ReadabilityRules, DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

        private static readonly Action<SyntaxNodeAnalysisContext> BlockAction = HandleBlock;

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(BlockAction, SyntaxKind.Block);
        }

        private static void HandleBlock(SyntaxNodeAnalysisContext context)
        {
            var block = context.Node as BlockSyntax;

            if (block != null && block.Statements.Any())
            {
                var previousStatement = block.Statements[0];
                var previousStatementLocation = previousStatement.GetLineSpan();
                FileLinePositionSpan currentStatementLocation;

                for (var i = 1; i < block.Statements.Count; i++)
                {
                    var currentStatement = block.Statements[i];
                    currentStatementLocation = currentStatement.GetLineSpan();

                    if (previousStatementLocation.EndLinePosition.Line
                        == currentStatementLocation.StartLinePosition.Line
                        && !IsLastTokenMissing(previousStatement))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, block.Statements[i].GetLocation()));
                    }

                    previousStatementLocation = currentStatementLocation;
                    previousStatement = currentStatement;
                }
            }
        }

        private static bool IsLastTokenMissing(StatementSyntax previousStatement)
        {
            return previousStatement.GetLastToken(includeZeroWidth: true, includeSkipped: true).IsMissing;
        }
    }
}