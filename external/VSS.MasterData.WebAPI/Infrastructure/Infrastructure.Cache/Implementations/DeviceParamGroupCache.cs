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
	public class DeviceParamGroupCache : IDeviceParamGroupCache
	{
		private readonly ILoggingService _loggingService;
		private readonly IInjectConfig _injectConfig;
		private readonly IDeviceParamGroupRepository _deviceParamGroupRepository;
		private ICache _parameterGroupCache;
		private IEnumerable<DeviceParamGroupDto> _dbResponse;

		public DeviceParamGroupCache(IDeviceParamGroupRepository deviceParamGroupRepository, IInjectConfig injectConfig, ILoggingService loggingService)
		{
			this._deviceParamGroupRepository = deviceParamGroupRepository;
			this._loggingService = loggingService;
			this._injectConfig = injectConfig;
			this._loggingService.CreateLogger(this.GetType());
			this._parameterGroupCache = this._injectConfig.ResolveKeyed<ICache>("DeviceParamGroupMemoryCache");
		}

		public async Task Initialize()
		{
			this._loggingService.Debug("Started updating Device Type Cache", "DeviceParamGroupCache.Initialize");
			this.UpdateParamGroups();
			this._loggingService.Debug("Ended updating Device Type Cache", "DeviceParamGroupCache.Initialize");
		}

		private List<DeviceParamGroupDto> UpdateParamGroups(string key = null) //If key is null, then cache will be refreshed
		{
			this._loggingService.Debug("Started executing method", "DeviceParamGroupCache.UpdateParamGroups");
			List<DeviceParamGroupDto> deviceParamGroupDtos = null;
			if (this._parameterGroupCache == null)
			{
				this._parameterGroupCache = this._injectConfig.ResolveKeyed<ICache>("DeviceParamGroupCache");
			}

			var DeviceParamGroupCache = this._parameterGroupCache.Get<List<DeviceTypeDto>>(key, null);
			if (DeviceParamGroupCache == null || !DeviceParamGroupCache.Any())
			{
				this._loggingService.Debug("Cache Miss, Started fetching from Database", "DeviceParamGroupCache.UpdateParamGroups");
				if (_dbResponse == null || !_dbResponse.Any())
				{
					_dbResponse = this._deviceParamGroupRepository.FetchAllDeviceParameterGroups(new DeviceParamGroupDto()).Result;
				}


				this._loggingService.Debug("Started building cache from Database", "DeviceParamGroupCache.UpdateParamGroups");
				foreach (var paramGroup in _dbResponse.GroupBy(x => x.Id))
				{
					var paramGroupList = paramGroup.ToList();
					if (key == paramGroup.Key.ToString())
					{
						deviceParamGroupDtos = paramGroupList;
					}
					this._parameterGroupCache.Upsert<List<DeviceParamGroupDto>>(paramGroup.Key.ToString(), paramGroupList);
				}
			}
			this._loggingService.Debug("Ended building cache from Database", "DeviceParamGroupCache.UpdateParamGroups");
			this._loggingService.Debug("Ended executing method", "DeviceParamGroupCache.UpdateParamGroups");
			return deviceParamGroupDtos;
		}

		public async Task<IEnumerable<DeviceParamGroupDto>> Get(string groupName)
		{
			var cached = this._parameterGroupCache.Get<List<DeviceParamGroupDto>>(groupName, this.UpdateParamGroups);
			return await Task.FromResult<IEnumerable<DeviceParamGroupDto>>(cached);
		}
	}
}