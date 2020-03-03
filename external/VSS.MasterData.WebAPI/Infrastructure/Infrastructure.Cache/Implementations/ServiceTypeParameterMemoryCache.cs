using System.Runtime.Caching;
using Utilities.Cache;

namespace Infrastructure.Cache.Implementations
{
	public class ServiceTypeParameterMemoryCache : InMemoryCache
	{
		public ServiceTypeParameterMemoryCache(string region = null, CacheItemPolicy cacheItemPolicy = null) : base(region, cacheItemPolicy) { }
	}
}
