using System.Runtime.Caching;
using Utilities.Cache;

namespace Infrastructure.Cache.Implementations
{
	public class ParameterAttributeMemoryCache : InMemoryCache
	{
		public ParameterAttributeMemoryCache(string region = null, CacheItemPolicy cacheItemPolicy = null) : base(region, cacheItemPolicy) { }
	}
}
