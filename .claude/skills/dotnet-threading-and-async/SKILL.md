---
name: dotnet-threading-and-async
description: Use when writing or modifying async code in a .NET library — `Task`/`ValueTask` continuations, `AsyncLocal` state, blocking vs non-blocking waits, conditional `Thread.Abort` code, or async test lifecycle. Covers the threading/async conventions that prevent deadlocks, context bleed, and intermittent runner failures.
---

# .NET threading and async

A .NET library may mix synchronous and asynchronous call sites in the same process, rely on `AsyncLocal` state that follows the logical call flow, and must work on target frameworks with and without `Thread.Abort`. Small mistakes here (blocking in async, capturing the wrong context, feature-gating on the wrong symbol) produce intermittent failures that are hard to diagnose.

## No blocking in async code

Inside an `async` method, never do synchronous waits. They consume a thread-pool thread for the duration of the wait and can deadlock when running under a synchronization context (UI frameworks, classic ASP.NET).

```csharp
// Wrong — blocks a thread
public async Task PollsUntilReady()
{
    Thread.Sleep(100);
    var ready = GetStatusAsync().Result;
    ...
}

// Right — yields the thread
public async Task PollsUntilReady()
{
    await Task.Delay(100);
    var ready = await GetStatusAsync();
    ...
}
```

Same rule for `.Wait()`, `Task.WaitAll`, `Task.WaitAny`, and `GetAwaiter().GetResult()` in async contexts.

## `AsyncLocal<T>` state follows logical flow — capture it deliberately

`AsyncLocal<T>` slots reflect *whatever logical operation is currently flowing through this code*. They are not a snapshot of when the surrounding code was written. If you capture an `AsyncLocal` value at the wrong moment — for example, inside a delegate that fires after an `await` resume — you may read state belonging to a *different* logical operation that has since reused the same thread.

```csharp
// Risky — the AsyncLocal value at await-resume time may differ from registration time
public void RegisterCallback()
{
    _callback = async () =>
    {
        var current = MyAmbientContext.Value;   // whose context?
        ...
    };
}

// Safer — capture at the moment you know the right context is active
public void RegisterCallback()
{
    var captured = MyAmbientContext.Value;
    _callback = async () =>
    {
        var current = captured;
        ...
    };
}
```

If you're writing a listener, callback, or anything that fires after an `await` point, capture the ambient state you need up front and close over it.

**Agent Warning:** Do not use `Task.Run()` to artificially wrap synchronous code just to make it async. This breaks logical-flow tracking and can cause unhandled exceptions to escape the calling context.

## `Thread.Abort` is only available on some target frameworks

`Thread.Abort` exists on .NET Framework and is gone on .NET 6+ (it throws `PlatformNotSupportedException` where still present). Code that depends on it must be feature-gated on a **feature constant**, not a platform symbol:

```csharp
#if THREAD_ABORT
    thread.Abort();
#else
    cancellationTokenSource.Cancel();
#endif
```

Define `THREAD_ABORT` in `Directory.Build.props` for the `net4*` TFMs. Don't use `#if NETFRAMEWORK` as the gate — feature-gating makes it explicit *why* the branch exists and survives adding or removing TFMs.

## Async test lifecycle

Returning `Task` (or `ValueTask`) from test methods and from setup/teardown lifecycle hooks is supported by all current .NET test frameworks and awaited by the runner.

**Do not return `async void`** from test lifecycle methods or from any method where you control the signature. An unobserved exception in `async void` is raised on the synchronization context and crashes the runner rather than failing the test.

The one legitimate exception is an **event handler** whose signature is fixed by the delegate (`EventHandler`, `PropertyChangedEventHandler`, etc.). You can't return `Task` there. If you need to do async work in an event handler, wrap the whole body in `try`/`catch` so the exception can't escape:

```csharp
private async void OnSomethingHappened(object sender, EventArgs e)
{
    try
    {
        await DoAsyncWorkAsync();
    }
    catch (Exception ex)
    {
        // Log, raise a listener event, or route into the owning component's state.
        // Never let an exception unwind out of async void.
        _log.Error(ex, "OnSomethingHappened failed");
    }
}
```

If you find yourself wanting `async void` anywhere else, change the signature to `async Task` instead.

## Cancellation and timeouts

Cancellation guidance depends on *where* you're writing the code.

**In tests and application-layer code:**
- Respect `CancellationToken` when a method you're calling already takes one; thread it through to the `await`.
- When a wait needs an upper bound, prefer `CancellationTokenSource.CancelAfter(...)` + pass the token to the awaited call, over wall-clock polling (`Thread.Sleep`, repeated `Task.Delay`).

**In library internals (polling utilities, wait loops, anything with load-bearing timing semantics):**
- **Do not rewrite existing polling or wait mechanisms to use `CancellationToken`.** Several of these have load-bearing timing semantics (poll intervals, final-check-before-fail behaviour, delay-until-quiet) that downstream callers depend on. Changing the mechanism is a behavioural change, not a refactor.
- Only use token-based bounding when you are **writing new** wait code that has no prior contract, or when the public API already exposes a `CancellationToken` parameter that you need to honour.
- If you genuinely need to modernise an existing polling loop, open a separate PR with benchmarks, not a drive-by refactor.

**On test-framework `[Timeout]` attributes:** these are best-effort and depend on the runtime's ability to interrupt the thread. On .NET Core+, a timed-out async test is *not* forcibly aborted — it is reported as failed and left running in the background. Design tests that use `[Timeout]` with that in mind; if a hang would leak resources, bound the wait inside the test with a token too.

## `ValueTask` vs `Task`

Public library API surface should use `Task` by default. Internal code may use `ValueTask` for allocation-sensitive hot paths, but only where the result is awaited exactly once. Do not store a `ValueTask` and await it twice, and do not convert a reference-type-returning method to `ValueTask<T>` for style — the memory optimisation only pays off for allocation-heavy sync completions.
