using System.Runtime.Caching;
using Utilities.Cache;

namespace Infrastructure.Cache.Implementations
{
	public class DeviceParamGroupMemoryCache : InMemoryCache
	{
		public DeviceParamGroupMemoryCache(string region = null, CacheItemPolicy cacheItemPolicy = null) : base(region, cacheItemPolicy) { }
	}
}
