using Infrastructure.Cache.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.IOC;

namespace AssetSettings
{
	public class StartUpCacheUpdater : IStartUpObject
	{
		private readonly IParameterAttributeCache _parameterAttributeCache;
		private readonly IServiceTypeParameterCache _serviceTypeParameterCache;
		
		public StartUpCacheUpdater(IInjectConfig injectConfig)
		{
			_parameterAttributeCache = injectConfig.Resolve<IParameterAttributeCache>();
			_serviceTypeParameterCache = injectConfig.Resolve<IServiceTypeParameterCache>();
		}
		
		public async Task Initialize()
		{
			Task parameterAttrCacheTask = _parameterAttributeCache.Initialize();
			Task serviceTypeAttrCacheTask = _serviceTypeParameterCache.Initialize();

			await Task.WhenAll(parameterAttrCacheTask, serviceTypeAttrCacheTask);
		}		
	}
}
