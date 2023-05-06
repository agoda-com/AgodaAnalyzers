namespace Agoda.Analyzers.Test.Helpers;

public static class ClassBuilder
{
    private static readonly string[] dataTypes = {"int", "long", "float", "double", "short", "byte", "sbyte", "bool", "decimal", "char", "ushort", "uint", "ulong"};
    public static string New()
    {
        const string that = "using System;\n";
        return that;
    }
        
    public static string WithNamespace(this string that, string nameSpace = "Agoda.Analyzers.Test")
    {
        that = $"{that}namespace {nameSpace}\n{{\n"; 
        return that;
    }
        
    public static string WithClass(this string that, string className = "TestClass", int numberOfPublicConstructors = 0, int numberOfPrivateConstructors = 0, string attribute = null, string inheritanceClassName = null)
    {
        if (!string.IsNullOrEmpty(attribute))
        {
            that = $"{that}\t[{attribute}]\n";
        }

        that = $"{that}\tpublic class {className}{(!string.IsNullOrEmpty(inheritanceClassName) ? $" : {inheritanceClassName}" : "")}\n\t{{ \n";

        for (var i = 0; i < numberOfPrivateConstructors; i++)
        {
            that = $"{that}\t\tprivate {className}({dataTypes[i]} value) {{}} \n";
        }
            
        for (var i = 0; i < numberOfPublicConstructors; i++)
        {
            that = $"{that}\t\tpublic {className}({dataTypes[dataTypes.Length - 1 - i]} value) {{}} \n";
        }
            
        that = $"{that}\t}}\n";
        return that;
    }
        
    public static string WithAttributeClass(this string that, string attributeName = "TestAttribute")
    {
        that = $"{that}\n\tinternal class {attributeName} : Attribute\n\t{{\n\t}}\n";
        return that;
    }
    public static string WithInheritanceClass(this string that, string inheritanceClassName = "TestInheritance")
    {
        that = $"{that}\n\tpublic class {inheritanceClassName}\n\t{{\n\t}}\n";
        return that;
    }

    public static string Build(this string that, string name = "TestClass")
    {
        that = $"{that} }}";
        return that;
    }
}