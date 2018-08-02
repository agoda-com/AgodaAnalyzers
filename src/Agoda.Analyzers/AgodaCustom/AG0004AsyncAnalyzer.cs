//using System.Collections.Immutable;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.Diagnostics;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Agoda.Analyzers.Helpers;

//namespace Agoda.Analyzers.AgodaCustom
//{
//    [DiagnosticAnalyzer(LanguageNames.CSharp)]
//    public class AG0004AsyncAnalyzer : DiagnosticAnalyzer
//    {
//        internal const string DiagnosticId = "AG0004";

//        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(CustomRulesResources.AG0004Title), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));
//        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(CustomRulesResources.AG0004MessageFormat), CustomRulesResources.ResourceManager, typeof(CustomRulesResources));
//        private static readonly LocalizableString Description = DescriptionContentLoader.GetAnalyzerDescription(nameof(AG0004AsyncAnalyzer));

//        private static readonly DiagnosticDescriptor Descriptor =
//            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, AnalyzerCategory.CustomQualityRules,
//                DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, null, WellKnownDiagnosticTags.EditAndContinue);


//        public override void Initialize(AnalysisContext context)
//        {
//            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.SimpleMemberAccessExpression);
//        }

//        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Descriptor); } }

//        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
//        {
//            var memberAccessNode = (MemberAccessExpressionSyntax)context.Node;

//            var method = context.SemanticModel.GetEnclosingSymbol(context.Node.SpanStart) as IMethodSymbol;

//            if (method != null && method.IsAsync)
//            {
//                var invokeMethod = context.SemanticModel.GetSymbolInfo(context.Node).Symbol as IMethodSymbol;

//                var location = memberAccessNode.Name.GetLocation();

//                if (invokeMethod != null && !invokeMethod.IsExtensionMethod)
//                {

//                    // Checks if the Wait method is called within an async method then creates the diagnostic.
//                    if (invokeMethod.OriginalDefinition.Name.Equals("Wait"))
//                    {
//                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
//                        return;
//                    }

//                    // Checks if the WaitAny method is called within an async method then creates the diagnostic.
//                    if (invokeMethod.OriginalDefinition.Name.Equals("WaitAny"))
//                    {
//                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
//                        return;
//                    }

//                    // Checks if the WaitAll method is called within an async method then creates the diagnostic.
//                    if (invokeMethod.OriginalDefinition.Name.Equals("WaitAll"))
//                    {
//                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
//                        return;
//                    }

//                    // Checks if the Sleep method is called within an async method then creates the diagnostic.
//                    if (invokeMethod.OriginalDefinition.Name.Equals("Sleep"))
//                    {
//                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
//                        return;
//                    }

//                    // Checks if the GetResult method is called within an async method then creates the diagnostic.     
//                    if (invokeMethod.OriginalDefinition.Name.Equals("GetResult"))
//                    {
//                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
//                        return;
//                    }
//                }

//                var property = context.SemanticModel.GetSymbolInfo(context.Node).Symbol as IPropertySymbol;

//                // Checks if the Result property is called within an async method then creates the diagnostic.
//                if (property != null && property.OriginalDefinition.Name.Equals("Result"))
//                {
//                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
//                    return;
//                }
//            }
//        }
//    }
//}
