# Rebuilding an argument list without losing layout

Read this when a fix inserts, removes or replaces arguments in a call and the other arguments must come
through unchanged.

## The shape

A `SeparatedSyntaxList<ArgumentSyntax>` stores the arguments and the commas between them. Line breaks
and comments are trivia on those arguments and commas:

```csharp
_logger.LogError(
    ex, // timeout from supplier        ← trailing trivia on the comma after `ex`
    $"Failed {id}");                    ← leading trivia (newline + indent) on the argument
```

`SyntaxFactory.SeparatedList(arguments)` creates new `, ` separators and throws that trivia away.
`SeparatedList<ArgumentSyntax>(IEnumerable<SyntaxNodeOrToken>)` lets you pass each separator yourself.

## Recipe: replace argument `k` with one argument plus `n` new ones

```csharp
var original = invocation.ArgumentList.Arguments;
var nodes = new List<SyntaxNodeOrToken>();

// 1. Arguments before the one being rewritten: original nodes and original separators.
for (var i = 0; i < k; i++)
{
    nodes.Add(original[i]);
    nodes.Add(original.GetSeparator(i));
}

// 2. The replacement takes over the trivia of the node it replaces.
nodes.Add(replacement.WithTriviaFrom(original[k]));

// 3. New arguments didn't exist before, so they get new separators.
foreach (var extra in newArguments)
{
    nodes.Add(SyntaxFactory.Token(SyntaxKind.CommaToken).WithTrailingTrivia(SyntaxFactory.Space));
    nodes.Add(extra);
}

// 4. Arguments after it: original separators and original nodes.
for (var i = k + 1; i < original.Count; i++)
{
    nodes.Add(original.GetSeparator(i - 1));
    nodes.Add(original[i]);
}

var newList = invocation.ArgumentList.WithArguments(SyntaxFactory.SeparatedList<ArgumentSyntax>(nodes));
return invocation.WithArgumentList(newList).WithTriviaFrom(invocation);
```

Notes:

- Keep the original `ArgumentListSyntax` and call `WithArguments` on it, so the parentheses keep their
  trivia too.
- `WithoutTrivia()` is fine on an expression you're moving **into** a new place (an interpolation hole
  becoming an argument), because its old trivia belonged to the old place. It's wrong on anything that
  stays where it was.
- To remove argument `k`, skip it and one adjacent separator: the one after it, or the one before it if
  `k` is last.

## Removing one element from a list

```csharp
// SeparatedSyntaxList
var newArgs = list.Arguments.Remove(target);          // keeps the other separators
// SyntaxList (attribute lists, members, statements)
var newLists = member.AttributeLists.Remove(list);
// SyntaxTokenList (modifiers)
var newModifiers = member.Modifiers.Remove(token);
```

When removing the last item leaves an empty container (`[]`, an empty `#region`), remove the container
and move its leading trivia to the next node, so a comment above it isn't lost.

## Test for it

```csharp
[Test]
public async Task PreservesTriviaOnArgumentsItDoesNotRewrite()
{
    var code = @"
_logger.LogError(
    ex, // timeout from supplier
    $""Failed {id}"");";
    var expected = @"
_logger.LogError(
    ex, // timeout from supplier
    ""Failed {Id}"", id);";
    await VerifyCodeFixAsync(code, expected);   // and the Fix All variant
}
```

Source: AgodaAnalyzers #86, #246.
