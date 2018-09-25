using System;
using System.Collections.Generic;
using System.Web;

namespace Agoda.Analyzers.Test.AgodaCustom.TestCases.AG0001
{
    public class AG0003References : GenericReferences
    {
        public override IEnumerable<Type> ReferenceDefinitions()
        {
            yield return typeof(HttpContext);
        }
    }
}
