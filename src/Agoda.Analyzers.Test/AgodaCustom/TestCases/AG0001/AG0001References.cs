using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Agoda.Analyzers.Test.AgodaCustom.TestCases.AG0001
{
    public class AG0001References : GenericReferences
    {
        public override IEnumerable<Type> ReferenceDefinitions()
        {
            yield return typeof(DependencyResolver);
        }
    }
}
