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
	public class ParameterAttributeCache : IParameterAttributeCache
	{
		private readonly ILoggingService _loggingService;
		private readonly IInjectConfig _injectConfig;
		private readonly IDeviceTypeParameterAttributeRepository _parameterAttributeRepository;
		private ICache _parameterAttributeCache;
		private IEnumerable<DeviceTypeParameterAttributeDto> _dbResponse;

		public ParameterAttributeCache(IDeviceTypeParameterAttributeRepository parameterAttributeRepository, IInjectConfig injectConfig, ILoggingService loggingService)
		{
			this._parameterAttributeRepository = parameterAttributeRepository;
			this._loggingService = loggingService;
			this._injectConfig = injectConfig;
			this._loggingService.CreateLogger(this.GetType());
			this._parameterAttributeCache = this._injectConfig.ResolveKeyed<ICache>("ParameterAttributeMemoryCache");
		}

		public async Task Initialize()
		{
			this._loggingService.Debug("Started updating Parameter Attribute Cache", "AutofacContainer.BuildParameterAttributeCache");
			this.UpdateDeviceTypes();
			this.UpdateDeviceTypeParameter();
			this.UpdateDeviceTypeGroups();
			this.UpdateDeviceTypeGroupsParameter();
			this._loggingService.Debug("Ended updating Parameter Attribute Cache", "AutofacContainer.BuildParameterAttributeCache");
		}

		private List<DeviceTypeParameterAttributeDto> UpdateDeviceTypes(string key = null) //if key is null, then cache will be refreshed
		{
			this._loggingService.Debug("Started executing method", "ParameterAttributeCache.UpdateDeviceTypes");
			List<DeviceTypeParameterAttributeDto> devicetypeParameterAttributes = null;
			if (this._parameterAttributeCache == null)
			{
				this._parameterAttributeCache = this._injectConfig.ResolveKeyed<ICache>("ParameterAttributeCache");
			}

			var deviceTypeCache = this._parameterAttributeCache.Get<List<DeviceTypeParameterAttributeDto>>(key, null);
			if (deviceTypeCache == null || !deviceTypeCache.Any())
			{
				this._loggingService.Debug("Cache Miss, Started fetching from Database", "ParameterAttributeCache.UpdateDeviceTypes");
				if (_dbResponse == null || !_dbResponse.Any())
				{
					_dbResponse = this._parameterAttributeRepository.Fetch(new DeviceTypeParameterAttributeDto()).Result;
				}


				this._loggingService.Debug("Started building cache from Database", "ParameterAttributeCache.UpdateDeviceTypes");
				foreach (var parameterAttribute in _dbResponse.GroupBy(x => x.TypeName))
				{
					var parameterAttributeLists = parameterAttribute.ToList();
					if (key == parameterAttribute.Key)
					{
						devicetypeParameterAttributes = parameterAttributeLists;
					}
					this._parameterAttributeCache.Upsert<List<DeviceTypeParameterAttributeDto>>(parameterAttribute.Key, parameterAttributeLists);
				}
			}
			this._loggingService.Debug("Ended building cache from Database", "ParameterAttributeCache.UpdateDeviceTypes");
			this._loggingService.Debug("Ended executing method", "ParameterAttributeCache.UpdateDeviceTypes");
			return devicetypeParameterAttributes;
		}

		private List<DeviceTypeParameterAttributeDto> UpdateDeviceTypeParameter(string key = null) //if key is null, then cache will be refreshed
		{
			this._loggingService.Debug("Started executing method", "ParameterAttributeCache.UpdateDeviceTypeParameter");
			List<DeviceTypeParameterAttributeDto> devicetypeParameterAttributes = null;
			if (this._parameterAttributeCache == null)
			{
				this._parameterAttributeCache = this._injectConfig.ResolveKeyed<ICache>("ParameterAttributeCache");
			}

			var deviceTypeGroupsCache = this._parameterAttributeCache.Get<List<DeviceTypeParameterAttributeDto>>(key, null);
			if (deviceTypeGroupsCache == null || !deviceTypeGroupsCache.Any())
			{
				this._loggingService.Debug("Cache Miss, Started fetching from Database", "ParameterAttributeCache.UpdateDeviceTypeParameter");
				if (_dbResponse == null || !_dbResponse.Any())
				{
					_dbResponse = this._parameterAttributeRepository.Fetch(new DeviceTypeParameterAttributeDto()).Result;
				}

				this._loggingService.Debug("Started building cache from Database", "ParameterAttributeCache.UpdateDeviceTypeParameter");
				foreach (var parameterAttribute in _dbResponse.GroupBy(x => new { x.TypeName, x.ParameterName }))
				{
					var parameterAttributeLists = parameterAttribute.ToList();
					var itemKey = string.Join(",", parameterAttribute.Key.TypeName, parameterAttribute.Key.ParameterName);
					if (key == itemKey)
					{
						devicetypeParameterAttributes = parameterAttributeLists;
					}
					this._parameterAttributeCache.Upsert<List<DeviceTypeParameterAttributeDto>>(itemKey, parameterAttributeLists);
				}
				this._loggingService.Debug("Ended building cache from Database", "ParameterAttributeCache.UpdateDeviceTypeParameter");
			}
			this._loggingService.Debug("Ended executing method", "ParameterAttributeCache.UpdateDeviceTypeParameter");
			return devicetypeParameterAttributes;
		}

		private List<DeviceTypeParameterAttributeDto> UpdateDeviceTypeGroupsParameter(string key = null)
		{
			this._loggingService.Debug("Started executing method", "ParameterAttributeCache.UpdateDeviceTypeGroupsParameter");
			List<DeviceTypeParameterAttributeDto> devicetypeParameterAttributes = null;
			if (this._parameterAttributeCache == null)
			{
				this._parameterAttributeCache = this._injectConfig.ResolveKeyed<ICache>("ParameterAttributeCache");
			}


			var deviceTypeGroupsAttributeCache = this._parameterAttributeCache.Get<List<DeviceTypeParameterAttributeDto>>(key, null);
			if (deviceTypeGroupsAttributeCache == null || !deviceTypeGroupsAttributeCache.Any())
			{
				this._loggingService.Debug("Cache Miss, Started fetching from Database", "ParameterAttributeCache.UpdateDeviceTypeGroupsParameter");
				if (_dbResponse == null || !_dbResponse.Any())
				{
					_dbResponse = this._parameterAttributeRepository.Fetch(new DeviceTypeParameterAttributeDto()).Result;
				}
				this._loggingService.Debug("Ended building cache from Database", "ParameterAttributeCache.UpdateDeviceTypeGroupsParameter");
				foreach (var parameterAttribute in _dbResponse.GroupBy(x => new { x.TypeName, x.GroupName, x.ParameterName }))
				{
					var parameterAttributeLists = parameterAttribute.ToList();
					var itemKey = string.Join(",", parameterAttribute.Key.TypeName, parameterAttribute.Key.GroupName, parameterAttribute.Key.ParameterName);
					if (key == itemKey)
					{
						devicetypeParameterAttributes = parameterAttributeLists;
					}
					this._parameterAttributeCache.Upsert<List<DeviceTypeParameterAttributeDto>>(itemKey, parameterAttributeLists);
				}
				this._loggingService.Debug("Ended building cache from Database", "ParameterAttributeCache.UpdateDeviceTypeGroupsParameter");
			}
			this._loggingService.Debug("Ended executing method", "ParameterAttributeCache.UpdateDeviceTypeGroupsParameter");
			return devicetypeParameterAttributes;
		}

		private List<DeviceTypeParameterAttributeDto> UpdateDeviceTypeGroups(string key = null)
		{
			this._loggingService.Debug("Started executing method", "ParameterAttributeCache.UpdateDeviceTypeGroups");
			List<DeviceTypeParameterAttributeDto> devicetypeParameterAttributes = null;
			if (this._parameterAttributeCache == null)
			{
				this._parameterAttributeCache = this._injectConfig.ResolveKeyed<ICache>("ParameterAttributeCache");
			}


			var deviceTypeGroupsAttributeCache = this._parameterAttributeCache.Get<List<DeviceTypeParameterAttributeDto>>(key, null);
			if (deviceTypeGroupsAttributeCache == null || !deviceTypeGroupsAttributeCache.Any())
			{
				this._loggingService.Debug("Cache Miss, Started fetching from Database", "ParameterAttributeCache.UpdateDeviceTypeGroupsParameter");
				if (_dbResponse == null || !_dbResponse.Any())
				{
					_dbResponse = this._parameterAttributeRepository.Fetch(new DeviceTypeParameterAttributeDto()).Result;
				}

				this._loggingService.Debug("Ended building cache from Database", "ParameterAttributeCache.UpdateDeviceTypeGroupsParameter");
				foreach (var parameterAttribute in _dbResponse.GroupBy(x => new { x.TypeName, x.GroupName }))
				{
					var parameterAttributeLists = parameterAttribute.ToList();
					var itemKey = string.Join(",", parameterAttribute.Key.TypeName, parameterAttribute.Key.GroupName);
					if (key == itemKey)
					{
						devicetypeParameterAttributes = parameterAttributeLists;
					}
					this._parameterAttributeCache.Upsert<List<DeviceTypeParameterAttributeDto>>(itemKey, parameterAttributeLists);
				}
				this._loggingService.Debug("Ended building cache from Database", "ParameterAttributeCache.UpdateDeviceTypeGroups");
			}
			this._loggingService.Debug("Ended executing method", "ParameterAttributeCache.UpdateDeviceTypeGroups");
			return devicetypeParameterAttributes;
		}

		public async Task<IEnumerable<DeviceTypeParameterAttributeDto>> Get(string deviceType)
		{
			var cached = this._parameterAttributeCache.Get<List<DeviceTypeParameterAttributeDto>>(deviceType, this.UpdateDeviceTypes);
			return await Task.FromResult<IEnumerable<DeviceTypeParameterAttributeDto>>(cached);
		}
		public async Task<IEnumerable<DeviceTypeParameterAttributeDto>> Get(string deviceType, string parameter)
		{
			string key = string.Join(",", deviceType, parameter);
			var cached = this._parameterAttributeCache.Get<List<DeviceTypeParameterAttributeDto>>(key, this.UpdateDeviceTypeParameter);
			return await Task.FromResult<IEnumerable<DeviceTypeParameterAttributeDto>>(cached);
		}
		public async Task<IEnumerable<DeviceTypeParameterAttributeDto>> Get(string deviceType, string parameterGroup, string parameter)
		{
			string key = string.Join(",", deviceType, parameterGroup, parameter);
			var cached = this._parameterAttributeCache.Get<List<DeviceTypeParameterAttributeDto>>(key, this.UpdateDeviceTypeGroupsParameter);
			return await Task.FromResult<IEnumerable<DeviceTypeParameterAttributeDto>>(cached);
		}
	}
}