using System.Runtime.Caching;
using Utilities.Cache;

namespace Infrastructure.Cache.Implementations
{
	public class DeviceTypeMemoryCache : InMemoryCache
	{
		public DeviceTypeMemoryCache(string region = null, CacheItemPolicy cacheItemPolicy = null) : base(region, cacheItemPolicy) { }
	}
}
