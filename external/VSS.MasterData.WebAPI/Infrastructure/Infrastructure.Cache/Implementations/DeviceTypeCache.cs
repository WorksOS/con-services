using DbModel.DeviceConfig;
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
	public class DeviceTypeCache : IDeviceTypeCache
	{
		private readonly ILoggingService _loggingService;
		private readonly IInjectConfig _injectConfig;
		private readonly IDeviceTypeRepository _deviceTypeRepository;
		private ICache _deviceTypeCache;
		private IEnumerable<DeviceTypeDto> _dbResponse;

		public DeviceTypeCache(IDeviceTypeRepository deviceTypeRepository, IInjectConfig injectConfig, ILoggingService loggingService)
		{
			this._deviceTypeRepository = deviceTypeRepository;
			this._loggingService = loggingService;
			this._injectConfig = injectConfig;
			this._loggingService.CreateLogger(this.GetType());
			this._deviceTypeCache = this._injectConfig.ResolveKeyed<ICache>("DeviceTypeMemoryCache");
		}

		public async Task Initialize()
		{
			this._loggingService.Debug("Started updating Device Type Cache", "DeviceTypeCache.Initialize");
			this.UpdateDeviceTypes();
			this._loggingService.Debug("Ended updating Device Type Cache", "DeviceTypeCache.Initialize");
		}

		private List<DeviceTypeDto> UpdateDeviceTypes(string key = null) //If key is null, then cache will be refreshed
		{
			this._loggingService.Debug("Started executing method", "DeviceTypeCache.UpdateDeviceTypes");
			List<DeviceTypeDto> devicetypeParameterAttributes = null;
			if (this._deviceTypeCache == null)
			{
				this._deviceTypeCache = this._injectConfig.ResolveKeyed<ICache>("DeviceTypeCache");
			}

			var deviceTypeCache = this._deviceTypeCache.Get<List<DeviceTypeDto>>(key, null);
			if (deviceTypeCache == null || !deviceTypeCache.Any())
			{
				this._loggingService.Debug("Cache Miss, Started fetching from Database", "DeviceTypeCache.UpdateDeviceTypes");
				if (_dbResponse == null || !_dbResponse.Any())
				{
					_dbResponse = this._deviceTypeRepository.FetchAllDeviceTypes(new DeviceTypeDto()).Result;
				}


				this._loggingService.Debug("Started building cache from Database", "DeviceTypeCache.UpdateDeviceTypes");
				foreach (var deviceType in _dbResponse.GroupBy(x => x.TypeName))
				{
					var deviceTypeList = deviceType.ToList();
					if (key == deviceType.Key)
					{
						devicetypeParameterAttributes = deviceTypeList;
					}
					this._deviceTypeCache.Upsert<List<DeviceTypeDto>>(deviceType.Key, deviceTypeList);
				}
			}
			this._loggingService.Debug("Ended building cache from Database", "DeviceTypeCache.UpdateDeviceTypes");
			this._loggingService.Debug("Ended executing method", "DeviceTypeCache.UpdateDeviceTypes");
			return devicetypeParameterAttributes;
		}

		public async Task<IEnumerable<DeviceTypeDto>> Get(string deviceType)
		{
			var cached = this._deviceTypeCache.Get<List<DeviceTypeDto>>(deviceType, this.UpdateDeviceTypes);
			return await Task.FromResult<IEnumerable<DeviceTypeDto>>(cached);
		}
	}
}