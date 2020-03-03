using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace VSS.MasterData.WebAPI.Data.MySql.Cache
{
	/// <summary>
	/// Static class to hold cache entries, Shared across AppDomain
	/// </summary>
	public static class CacheEntry
	{
		static CacheEntry()
		{
			TypeCache = new ConcurrentDictionary<Type, Dictionary<string, string>>();
		}
		public static ConcurrentDictionary<Type, Dictionary<string, string>> TypeCache { get; set; }
	}
}
