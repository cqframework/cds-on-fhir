using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Spark.Core;

namespace GuidanceService.Store
{
	public static class SimpleInfrastructure
	{
        public static Infrastructure AddSimpleStore(this Infrastructure infrastructure)
        {
			var store = new SimpleStore();
            infrastructure.Store = store;
            infrastructure.Generator = store;
            infrastructure.SnapshotStore = store;
            infrastructure.Index = store;

            return infrastructure;
        }
	}
}