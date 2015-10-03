using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spark.Engine.Core;

namespace GuidanceService.Store
{
	internal class SimpleSnapshotCache : ISnapshotCache
	{
		private object _syncHandle = new object();
		private Dictionary<string, Snapshot> _cache = new Dictionary<string, Snapshot>();

		public void Add(Snapshot snapshot)
		{
			lock (_syncHandle)
			{
				_cache.Add(snapshot.Id, snapshot);
			}
		}

		public Snapshot Get(string snapshotId)
		{
			Snapshot snapshot;
			lock (_syncHandle)
			{
				if (_cache.TryGetValue(snapshotId, out snapshot))
				{
					return snapshot;
				}
			}

			throw new InvalidOperationException("Unknown snapshot identifier");
		}
	}
}
