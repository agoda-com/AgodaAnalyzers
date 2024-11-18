
Here is a template to use for markdown, followed by the description of the rule rule please create the markdown for this following the template example.
Be concise provide examples of non compliant as well as compliant code, if the rule is a blocker (meaning the compliant example is not needed because the rule says "dont use this"), then explain why we should not write code like this and the risks
follow standard common linting rules for markdown

MARKDOWN TEMPLATE HERE
---

# Type inheritance should not be recursive

ID: AG0000

Type: Vulnerability / Bug / Code Smell / Quick Fix

Recursion is a technique used to define a problem in terms of the problem itself, usually in terms of a simpler version of the problem itself.

For example, the implementation of the generator for the n-th value of the Fibonacci sequence comes naturally from its mathematical definition, when recursion is used:

```csharp
int NthFibonacciNumber(int n)
{
    if (n <= 1)
    {
        return 1;
    }
    else
    {
        return NthFibonacciNumber(n - 1) + NthFibonacciNumber(n - 2);
    }
}
```

As opposed to:

```csharp
int NthFibonacciNumber(int n)
{
    int previous = 0;
	int last = 1;
	for (var i = 0; i < n; i++)
	{
        (previous, last) = (last, last + previous);
	}
	return last;
}
```

The use of recursion is acceptable in methods, like the one above, where you can break out of it.

```csharp
int NthFibonacciNumber(int n)
{
    if (n <= 1)
    {
        return 1; // Base case: stop the recursion
    }
    // ...
}
```

It is also acceptable and makes sense in some type definitions:

```csharp
class Box : IComparable<Box>
{
    public int CompareTo(Box? other)
    {
        // Compare the two Box instances...
    }
}
```

With types, some invalid recursive definitions are caught by the compiler:

```csharp
class C2<T> : C2<T>     // Error CS0146: Circular base type dependency
{
}

class C2<T> : C2<C2<T>> // Error CS0146: Circular base type dependency
{
}
```

In more complex scenarios, however, the code will compile but execution will result in a TypeLoadException if you try to instantiate the class.

```csharp
class C1<T>
{
}

class C2<T> : C1<C2<C2<T>>> // Noncompliant
{
}

var c2 = new C2<int>();     // This would result into a TypeLoadException
```

WRITING STYLE TO FOLLOW
---

## Avoid blocking when possible

- Increase performance and scalability of your code by avoiding blocking calls.
- A blocking call ties up the thread while it waits for a response. During this time, it could have been doing something more useful, like serving other requests or redrawing the GUI.
- A non-blocking call returns the thread to the threadpool, or keeps the GUI thread responsive, while the task completes.

#### Don't
```c#
Thread.Sleep(5000); // thread is blocked for 5 seconds
```

#### Do
```c#
await Task.Delay(5000); // thread can do other stuff for 5 seconds.
```

#### Don't
```c#
public void CreateCsv()
{
    using(var writer = File.CreateText("myfile.csv"))
    {
        writer.WriteLine("...");
    }
}
```

#### Do
```c#
public async Task CreateCsvAsync()
{
    using(var writer = File.CreateText("myfile.csv"))
    {
        await writer.WriteLineAsync("...");
    }
}
```
## Use attribute-based routing instead of convention-based routing

- Convention-based routing tightly couples your code to your URLs. By default, renaming an action method would change
the URL it handles.
- Attribute-based routing decouples your class and method names from URLs, giving you the freedom to refactor
without breaking the site, or at least having to implement redirects.
- Attribute-based routing is arguably easier to understand, as the URL appears right above the code to which it points.
- For these reasons, always use attribute-based routing when exposing HTTP endpoints.

#### Don't

```c#
public class Global
{
    public void Application_Start()  
    {   
       RouteConfig.RegisterRoutes(RouteTable.Routes);  
    }
    
    public static void RegisterRoutes(RouteCollection routes)  
    {  
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}");  

        routes.MapRoute(  
            name: "Default",  
            url: "{controller}/{action}/{id}",  
            defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }  
        );  
    }  
}  
```

#### Do
```c#
public class HomeController: Controller  
{  
    [Route("{department}/employees/{employeeId ?}")]  
    public string Employee(string department, int? employeeId)  
    {  
        ...
    }  
}  
```


Description of rule
---
