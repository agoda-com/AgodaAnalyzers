﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Agoda.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Agoda.Analyzers.StyleCop
{
    /// <summary>
    /// The C# code contains an extra semicolon.
    /// </summary>
    /// <remarks>
    /// <para>A violation of this rule occurs when the code contain an extra semicolon. Syntactically, this results in
    /// an extra, empty statement in the code.</para>
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SA1106CodeMustNotContainEmptyStatements : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="SA1106CodeMustNotContainEmptyStatements"/> analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = "SA1106";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(ReadabilityResources.SA1106Title), ReadabilityResources.ResourceManager, typeof(ReadabilityResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(ReadabilityResources.SA1106MessageFormat), ReadabilityResources.ResourceManager, typeof(ReadabilityResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(ReadabilityResources.SA1106Description), ReadabilityResources.ResourceManager, typeof(ReadabilityResources));
        private static readonly string HelpLink = "https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1106.md";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DIAGNOSTIC_ID, Title, MessageFormat, AnalyzerCategory.ReadabilityRules, DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, HelpLink, WellKnownDiagnosticTags.Unnecessary);

        private static readonly Action<SyntaxNodeAnalysisContext> EmptyStatementAction = HandleEmptyStatement;
        private static readonly Action<SyntaxNodeAnalysisContext> BaseTypeDeclarationAction = HandleBaseTypeDeclaration;
        private static readonly Action<SyntaxNodeAnalysisContext> NamespaceDeclarationAction = HandleNamespaceDeclaration;

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(EmptyStatementAction, SyntaxKind.EmptyStatement);
            context.RegisterSyntaxNodeAction(BaseTypeDeclarationAction, SyntaxKinds.BaseTypeDeclaration);
            context.RegisterSyntaxNodeAction(NamespaceDeclarationAction, SyntaxKind.NamespaceDeclaration);
        }

        private static void HandleBaseTypeDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = (BaseTypeDeclarationSyntax) context.Node;

            if (declaration.SemicolonToken.IsKind(SyntaxKind.SemicolonToken))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, declaration.SemicolonToken.GetLocation(), properties: _props.ToImmutableDictionary()));
            }
        }

        private static void HandleNamespaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = (NamespaceDeclarationSyntax) context.Node;

            if (declaration.SemicolonToken.IsKind(SyntaxKind.SemicolonToken))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, declaration.SemicolonToken.GetLocation(), properties: _props.ToImmutableDictionary()));
            }
        }

        private static void HandleEmptyStatement(SyntaxNodeAnalysisContext context)
        {
            var syntax = (EmptyStatementSyntax) context.Node;

            var labeledStatementSyntax = syntax.Parent as LabeledStatementSyntax;
            if (labeledStatementSyntax != null)
            {
                var blockSyntax = labeledStatementSyntax.Parent as BlockSyntax;
                if (blockSyntax != null)
                {
                    for (var i = blockSyntax.Statements.Count - 1; i >= 0; i--)
                    {
                        var statement = blockSyntax.Statements[i];

                        // allow an empty statement to be used for a label, but only if no non-empty statements exist
                        // before the end of the block
                        if (blockSyntax.Statements[i] == labeledStatementSyntax)
                        {
                            return;
                        }

                        if (!statement.IsKind(SyntaxKind.EmptyStatement))
                        {
                            break;
                        }
                    }
                }
            }

            // Code must not contain empty statements
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, syntax.GetLocation(), properties: _props.ToImmutableDictionary()));
        }

        private static Dictionary<string, string> _props = new Dictionary<string, string>()
        {
            { AnalyzerConstants.KEY_TECH_DEBT_IN_MINUTES, "10" }
        };
    }
}