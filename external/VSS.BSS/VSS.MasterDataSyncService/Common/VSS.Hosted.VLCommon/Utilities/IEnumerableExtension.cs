using System;
using System.Collections.Generic;
using System.Linq;

namespace VSS.Hosted.VLCommon
{
  public static class IEnumerableExtension
  {
    /// <summary>
    /// This is used as a helper to help with omitting null checks while doing a foreach loop on an IEnumerable.
    /// </summary>
    public static IEnumerable<T> AsNotNull<T>(this IEnumerable<T> original)
    {
      return original ?? new T[0];
    }

		#region Enumerable Slicing
		private static IEnumerable<T> TakeOnEnumerator<T>(IEnumerator<T> enumerator, int count,
				Action<bool> setEndOfEnumeration)
		{
			bool moveNext = false;
			while (--count > 0 && (moveNext = enumerator.MoveNext()))
			{
				yield return enumerator.Current;
			}

			if (count > 0 && !moveNext)
			{
				setEndOfEnumeration(true);
			}
		}

		/// <summary>
		/// Used to perform pagination of an IEnumerable. Thread safe.
		/// Usage: foreach (var chunk in list.Chunk(chunkSize)) { do something with the subset }
		/// </summary>
		public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> items, int chunkSize)
		{
			if (chunkSize <= 0)
			{
				throw new ArgumentException("Chunks cannot be smaller than 1");
			}

			bool endOfEnumeration = false;
			using (var enumerator = items.GetEnumerator())
			{
				while (!endOfEnumeration)
				{
					yield return TakeOnEnumerator(enumerator, chunkSize, x => { endOfEnumeration = x; });
				}
			}
		}
		#endregion
	}
}
