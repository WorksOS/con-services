using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using ClientModel.DeviceConfig.Request;
using ClientModel.DeviceConfig.Request.DeviceConfig;
using CommonModel.DeviceSettings;
using CommonModel.DeviceSettings.ConfigNameValues;
using DeviceConfigRepository.MySql;
using DeviceConfigRepository.MySql.DeviceConfig;
using Infrastructure.Cache.Implementations;
using Infrastructure.Cache.Interfaces;
using Infrastructure.Service.DeviceAcknowledgementByPasser;
using Infrastructure.Service.DeviceAcknowledgementByPasser.Interfaces;
using Infrastructure.Service.DeviceConfig.Implementations;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Infrastructure.Service.DeviceConfig.Validators;
using Infrastructure.Service.DeviceMessageConstructor;
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Utilities.Cache;
using Utilities.IOC;
using Utilities.Logging;
using Utilities.Logging.Models;

namespace VSS.MasterData.WebAPI.Device.DeviceConfig
{
	public class DeviceConfigDependencyInjection : InjectConfigBase
	{
		public void InitializeStartupObjects()
		{
			var startUpObjects = this.Resolves<IStartUpObject>();

			//TODO: Add async version if needed
			foreach (var startUpObject in startUpObjects)
			{
				startUpObject.Initialize();
			}
		}
		private T BuildConfigNameValueCollection<T>(string sectionName, bool reverse) where T : ConfigNameValueCollection, new()
		{
			T result = new T();
			result.Values = Startup.Configuration.GetSection(sectionName).GetChildren()
				.Select(item => reverse ? new KeyValuePair<string, string>(item.Value, item.Key) : new KeyValuePair<string, string>(item.Key, item.Value))
				.ToDictionary(x => x.Key, x => x.Value);
			return result;
		}


		public override void ConfigureServiceCollection(IServiceCollection services)
		{

			#region Log - Configuration - StartUpObjects

			services.AddSingleton<IInjectConfig>(this);

			services.Configure<Configurations>(Startup.Configuration);

			services.AddScoped<ILoggingService, LoggingService>();

			services.AddScoped<LogRequestContext>();

			services.AddScoped<IStartUpObject, StartUpCacheUpdater>();

			#endregion

			#region Cache 

			services.AddSingleton<ICache>(new ParameterAttributeMemoryCache("ParameterAttributeCache",
				new CacheItemPolicy
				{
					Priority = CacheItemPriority.NotRemovable
				}
			));
			services.AddSingleton<ICache>(new ServiceTypeParameterMemoryCache("ServiceTypeParameterCache",
				new CacheItemPolicy
				{
					Priority = CacheItemPriority.NotRemovable
				}
			));
			services.AddSingleton<ICache>(new DeviceTypeMemoryCache("DeviceTypeCache",
				new CacheItemPolicy
				{
					Priority = CacheItemPriority.NotRemovable
				}
			));
			services.AddSingleton<ICache>(new DeviceParamGroupMemoryCache("DeviceParamGroupCache",
				new CacheItemPolicy
				{
					Priority = CacheItemPriority.NotRemovable
				}
			));

			services.AddScoped<IDeviceTypeParameterAttributeRepository, DeviceTypeParameterAttributeRepository.DeviceTypeParameterAttributeRepository>();
			services.AddScoped<IServiceTypeParameterRepository, ServiceTypeParameterRepository>();
			services.AddScoped<IParameterAttributeCache, ParameterAttributeCache>();
			services.AddScoped<IServiceTypeParameterCache, ServiceTypeParameterCache>();
			services.AddScoped<IDeviceTypeCache, DeviceTypeCache>();
			services.AddScoped<IDeviceParamGroupCache, DeviceParamGroupCache>();
			services.AddScoped<IDeviceParamGroupRepository, DeviceParamGroupRepository>();
			#endregion

			#region ConfigNameValueCollection & ConfigListsCollection

			//UIParameter to DBAttribute Mapper
			services.AddSingleton<DeviceConfigRequestToAttributeMaps>(this.BuildConfigNameValueCollection<DeviceConfigRequestToAttributeMaps>("deviceConfigParameterAttributeMaps", false));
			services.AddSingleton<DeviceConfigAttributeToRequestMaps>(this.BuildConfigNameValueCollection<DeviceConfigAttributeToRequestMaps>("deviceConfigParameterAttributeMaps", true));
			services.AddSingleton<DeviceConfigParameterNames>(this.BuildConfigNameValueCollection<DeviceConfigParameterNames>("deviceConfigParameterName", false));


			services.AddSingleton<ConfigListsCollection>(new ParameterGroupsNonMandatoryLists
			{
				Values = new List<string>(Startup.Configuration["AppSettings:DeviceConfigParameterGroupsNonMandatoryLists"].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
			});

			services.AddSingleton<ConfigListsCollection>(new ParametersNonMandatoryLists
			{
				Values = new List<string>(Startup.Configuration["AppSettings:DeviceConfigParametersNonMandatoryLists"].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
			});

			services.AddSingleton<ConfigListsCollection>(new AttributesNonMandatoryLists
			{
				Values = new List<string>(Startup.Configuration["AppSettings:DeviceConfigAttributesNonMandatoryLists"].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
			});


			#endregion

			#region Validators

			//Service Request Validators

			services.AddScoped<IRequestValidator<IServiceRequest>, CustomerUIDValidator>();
			services.AddScoped<IRequestValidator<IServiceRequest>, UserUIDValidator>();


			//Device Config Request Validators

			services.AddScoped<IRequestValidator<DeviceConfigRequestBase>, DeviceTypeValidator>();
			services.AddScoped<IRequestValidator<DeviceConfigRequestBase>, DeviceTypeParameterGroupValidator>();
			services.AddScoped<IRequestValidator<DeviceConfigRequestBase>, AssetUIDsValidator>();
			services.AddScoped<IRequestValidator<DeviceConfigRequestBase>, AllAttributeAsMandatoryValidator>();

			#endregion

			services.AddScoped<IAssetDeviceRepository, AssetDeviceRepository>();
			services.AddScoped<IMessageConstructor, MessageConstructor>();

			//Acknowledgement Bypasser
			services.AddScoped<IAckBypasser, AcknowledgementBypasser>();

			//Device Config Settings Base
			services.AddScoped<IDeviceConfigSettingConfig, DeviceConfigSettingConfigBase>();

			services.AddScoped<IGroupSettingsConfig, MetersSettingsConfig>();


			#region Repository

			//Repository 
			services.AddScoped<IDeviceConfigRepository, DeviceConfigRepository.MySql.DeviceConfig.DeviceConfigRepository>();
			services.AddScoped<IUserAssetRepository, UserAssetRepository>();
			services.AddScoped<IDeviceTypeRepository, DeviceTypeRepository>();
			services.AddScoped<ISubscriptionServicePlanRepository, SubscriptionServicePlanRepository>();

			#endregion


			#region Ping

			services.AddScoped<IServiceRequest, DevicePingLogRequest>();
			services.AddScoped<IDevicePingService, DevicePingService>();
			services.AddScoped<IRequestValidator<DevicePingLogRequest>, PingValidator>();
			services.AddScoped<IDevicePingRepository, DevicePingRepository>();

			#endregion
		}
	}
}
