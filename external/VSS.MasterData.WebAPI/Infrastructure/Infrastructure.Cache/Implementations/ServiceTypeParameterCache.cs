using DbModel.Cache;
using Infrastructure.Cache.Interfaces;
using Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Cache;
using Utilities.IOC;
using Utilities.Logging;

namespace Infrastructure.Cache.Implementations
{
	public class ServiceTypeParameterCache : IServiceTypeParameterCache
	{
		private readonly ILoggingService _loggingService;
		private readonly IInjectConfig _injectConfig;
		private readonly ICache _serviceTypeParameterCache;
		private readonly IServiceTypeParameterRepository _serviceTypeParameterRepository;
		private const string _keyFormat = "{0}:{1}";
		public ServiceTypeParameterCache(IServiceTypeParameterRepository serviceTypeParameterRepository, IInjectConfig injectConfig, ILoggingService loggingService)
		{
			this._serviceTypeParameterRepository = serviceTypeParameterRepository;
			this._loggingService = loggingService;
			this._injectConfig = injectConfig;
			this._loggingService.CreateLogger(this.GetType());
			this._serviceTypeParameterCache = this._injectConfig.ResolveKeyed<ICache>("ServiceTypeParameterMemoryCache");
		}

		public async Task Initialize()
		{
			this._loggingService.Info("Started updating Service Type Parameter Cache", "ServiceTypeParameterCache.Update");
			this.UpdateServiceTypeForParameter();
			this.UpdateServiceTypeForParameterServiceType();
			this._loggingService.Info("Ended updating Service Type Parameter Cache", "ServiceTypeParameterCache.Update");
		}

		private IEnumerable<ServiceTypeParameterDto> UpdateServiceTypeForParameter(string requestKey = null) //if requestKey is null, then cache will be refreshed
		{
			List<ServiceTypeParameterDto> result = new List<ServiceTypeParameterDto>();
			var serviceTypeParameters = this._serviceTypeParameterRepository.FetchAllServiceTypeParameter().Result;
			foreach(var parameterServiceTypes in serviceTypeParameters.GroupBy(x => x.DeviceParameterName))
			{
				this._serviceTypeParameterCache.Upsert(parameterServiceTypes.Key, parameterServiceTypes.ToList());
				if (parameterServiceTypes.Key == requestKey)
				{
					result = parameterServiceTypes.ToList();
				}
			}
			return new List<ServiceTypeParameterDto>(result);
		}
		private ServiceTypeParameterDto UpdateServiceTypeForParameterServiceType(string requestKey = null) //if requestKey is null, then cache will be refreshed
		{
			var serviceTypeParameters = this._serviceTypeParameterRepository.FetchAllServiceTypeParameter().Result;
			ServiceTypeParameterDto result = null;
			foreach (var parameterServiceTypes in serviceTypeParameters.GroupBy(x => x.DeviceParameterName))
			{
				foreach (var parameterServiceType in parameterServiceTypes)
				{
					var key = string.Format(_keyFormat, parameterServiceType.DeviceParameterName, parameterServiceType.ServiceTypeName);
					if(key == requestKey)
					{
						result = parameterServiceType;
					}
					this._serviceTypeParameterCache.Upsert(key, parameterServiceType);
				}
			}
			return result;
		}

		public async Task<ServiceTypeParameterDto> Get(string deviceParameterName, string serviceTypeName)
		{
			var result = this._serviceTypeParameterCache.Get<ServiceTypeParameterDto>(string.Format(_keyFormat, deviceParameterName, serviceTypeName), this.UpdateServiceTypeForParameterServiceType);
			return result;
		}

		public async Task<IEnumerable<ServiceTypeParameterDto>> Get(string deviceParameterName)
		{
			var result = this._serviceTypeParameterCache.Get<IEnumerable<ServiceTypeParameterDto>>(deviceParameterName, this.UpdateServiceTypeForParameter);
			return result;
		}
	}
}
