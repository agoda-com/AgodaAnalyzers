using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agoda.Analyzers.Test.Helpers.GenericTestHelpers
{
    public abstract class GenericReferences
    {
        public abstract IEnumerable<Type> ReferenceDefinitions();
    }
}
