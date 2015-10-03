using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spark.Engine.Core;

namespace GuidanceService.Store
{
	public interface ISnapshotCache
	{
		void Add(Snapshot snapshot);
		Snapshot Get(string snapshotId);
	}
}
