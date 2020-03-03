using System;
using System.Collections.Concurrent;

namespace Rebus.Internals
{
	internal sealed class RebusFeature
	{
		public ConcurrentDictionary<string, bool> Groups { get; } = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
	}
}
