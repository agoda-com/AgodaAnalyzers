using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agoda.Analyzers.Test.Helpers.GenericTestHelpers
{
    /// <summary>
    /// All test cases that want to include reference to assemblies, must inherit this class
    /// </summary>
    public abstract class GenericReferences
    {
        /// <summary>
        /// All external assemblies that the test case needs to reference
        /// </summary>
        public abstract IEnumerable<Type> ReferenceDefinitions { get; }
    }
}
