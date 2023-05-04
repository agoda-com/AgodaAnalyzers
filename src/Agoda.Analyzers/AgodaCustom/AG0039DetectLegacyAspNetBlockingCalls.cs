//using System.Collections.Immutable;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.CodeAnalysis.Diagnostics;

//[DiagnosticAnalyzer(LanguageNames.CSharp)]
//public class AG0039NonAsyncMethodFix : DiagnosticAnalyzer
//{
//    private const string Category = "Async";

//    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
//        id: "ASYNC001",
//        title: "Avoid blocking calls in non-async methods",
//        messageFormat: "Consider making '{0}' an async method and using 'await' instead of '{1}'.",
//        category: Category,
//        defaultSeverity: DiagnosticSeverity.Warning,
//        isEnabledByDefault: true,
//        description: "Blocking calls in non-async methods can cause performance problems and lead to deadlocks.");

//    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

//    public override void Initialize(AnalysisContext context)
//    {
//        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
//    }

//    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
//    {
//        var method = (MethodDeclarationSyntax)context.Node;

//        if (method.Modifiers.Any(SyntaxKind.AsyncKeyword))
//        {
//            // Method is already async, so no need to check for blocking calls
//            return;
//        }

//        var invocationExpressions = method.DescendantNodes()
//            .OfType<InvocationExpressionSyntax>();

//        foreach (var invocation in invocationExpressions)
//        {
//            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;

//            if (memberAccess == null)
//            {
//                // Not a method call, so skip
//                continue;
//            }

//            var methodSymbol = context.SemanticModel.GetSymbolInfo(memberAccess.Name).Symbol as IMethodSymbol;

//            if (methodSymbol?.ReturnType?.Name != "ValueTask" || invocation.ArgumentList.Arguments.Count != 1)
//            {
//                // Not a blocking call to a ValueTask method, so skip
//                continue;
//            }

//            var newMethod = method.WithReturnType(SyntaxFactory.ParseTypeName("Task<" + methodSymbol.ReturnType.Name + ">"))
//                .WithModifiers(method.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.AsyncKeyword)));

//            var awaitExpression = SyntaxFactory.AwaitExpression(invocation.WithoutTrivia().WithExpression(memberAccess.Expression))
//                .WithTriviaFrom(invocation);

//            var newBody = method.Body.WithStatements(
//                SyntaxFactory.SingletonList<StatementSyntax>(
//                    SyntaxFactory.ReturnStatement(awaitExpression)));

//            var newMethodWithBody = newMethod.WithBody(newBody);

//            var diagnostic = Diagnostic.Create(Rule, method.GetLocation(), method.Identifier, invocation);

//            context.ReportDiagnostic(diagnostic);

//            context.RegisterCodeFix(
//                Microsoft.CodeAnalysis.CodeActions.CodeAction.Create(
//                    title: "Convert to async method",
//                    createChangedDocument: c => Microsoft.CodeAnalysis.CodeActions.DocumentEditor.CreateAsync(context.Document, c =>
//                    {
//                        var editor = Microsoft.CodeAnalysis.Editing.DocumentEditor.CreateAsync(context.Document, c).Result;
//                        editor.ReplaceNode(method, newMethodWithBody);
//                        return Task.FromResult(editor.GetChangedDocument());
//                    }),
//                    equivalenceKey: "Convert to async method"
//                ),
//                diagnostic
//            );
//        }
//    }
//}


using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace Agoda.Analyzers.AgodaCustom
{


    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AG0039DetectLegacyAspNetBlockingCalls : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AC0039";
        private const string Title = "Blocking call in non-async method";
        private const string MessageFormat = "Method is blocking call, maybe should be async.";
        private const string Description = "Async methods should not be blocked on.";

        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, true, description: Description);

        public static DiagnosticDescriptor MyRule => Rule;
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if (!IsTaskResultCall(invocation))
            {
                return;
            }

            var containingMethod = invocation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (containingMethod == null || containingMethod.Modifiers.Any(SyntaxKind.AsyncKeyword))
            {
                return;
            }

            var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }

        private static bool IsTaskResultCall(InvocationExpressionSyntax invocation)
        {
            return invocation.Parent is MemberAccessExpressionSyntax memberAccess &&
                   memberAccess.Name.Identifier.ValueText == "Result";
        }
    }

}