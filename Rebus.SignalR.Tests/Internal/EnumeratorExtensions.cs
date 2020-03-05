using System.Collections.Generic;

namespace Rebus.SignalR.Tests.Internal
{
    internal static class EnumeratorExtensions
	{
        public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }
    }
}