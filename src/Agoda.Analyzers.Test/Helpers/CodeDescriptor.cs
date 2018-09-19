using System.Collections.Generic;
using System.Reflection;

namespace Agoda.Analyzers.Test.Helpers
{
    public class CodeDescriptor
    {
        public string Code { get; set; } = "";
        public IEnumerable<Assembly> References { get; set; } = new List<Assembly>();
        
        public CodeDescriptor()
        { }
        
        public CodeDescriptor(string code)
        {
            Code = code;
        }
    }
}