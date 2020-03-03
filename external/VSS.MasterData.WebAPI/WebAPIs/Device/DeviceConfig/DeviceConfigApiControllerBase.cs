using CommonApiLibrary;
using CommonModel.DeviceSettings.ConfigNameValues;
using Microsoft.Extensions.Logging;
using Utilities.Logging;

namespace VSS.MasterData.WebAPI.Device.DeviceConfig
{
	public class DeviceConfigApiControllerBase : ApiControllerBase
	{
		protected readonly ILogger _loggingService;
		protected readonly ConfigNameValueCollection _attributeMaps;

		public DeviceConfigApiControllerBase(ConfigNameValueCollection attributeMaps, ILogger loggingService)
		{
			this._loggingService = loggingService;
			this._attributeMaps = attributeMaps;
		}
	}
}
