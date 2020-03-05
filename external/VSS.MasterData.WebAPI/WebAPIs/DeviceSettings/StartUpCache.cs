using Infrastructure.Cache.Interfaces;
using System.Threading.Tasks;
using Utilities.IOC;

namespace DeviceSettings
{
	public class StartUpCacheUpdater : IStartUpObject
	{
		private readonly IParameterAttributeCache _parameterAttributeCache;
		private readonly IServiceTypeParameterCache _serviceTypeParameterCache;
		private readonly IDeviceTypeCache _deviceTypeCache;
		private readonly IDeviceParamGroupCache _deviceParamGroupCache;

		public StartUpCacheUpdater(IInjectConfig injectConfig)
		{
			_parameterAttributeCache = injectConfig.Resolve<IParameterAttributeCache>();
			_serviceTypeParameterCache = injectConfig.Resolve<IServiceTypeParameterCache>();
			_deviceTypeCache = injectConfig.Resolve<IDeviceTypeCache>();
			_deviceParamGroupCache = injectConfig.Resolve<IDeviceParamGroupCache>();
		}
		
		public async Task Initialize()
		{
			Task parameterAttrCacheTask = _parameterAttributeCache.Initialize();
			Task serviceTypeAttrCacheTask = _serviceTypeParameterCache.Initialize();
			Task deviceTypeCacheTask = _deviceTypeCache.Initialize();
			Task paramGroupCacheTask = _deviceParamGroupCache.Initialize();
			await Task.WhenAll(parameterAttrCacheTask, serviceTypeAttrCacheTask, deviceTypeCacheTask, paramGroupCacheTask);
		}		
	}
}
