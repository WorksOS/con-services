using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Request.DeviceConfig.AssetSecurity;
using ClientModel.DeviceConfig.Response.DeviceConfig.Asset_Security;
using CommonApiLibrary.Filters;
using CommonModel.DeviceSettings.ConfigNameValues;
using CommonModel.Error;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Utilities.IOC;
using Utilities.Logging;

namespace DeviceSettings.Controller
{
    [Route("v1/deviceconfigs/assetsecurity")]
    public class DeviceConfigAssetSecurityController : DeviceConfigApiControllerBase
    {
        private readonly IDeviceConfigService<DeviceConfigAssetSecurityRequest, DeviceConfigAssetSecurityDetails> _assetSecurityService;
        private readonly string _parameterGroupName = "AssetSecurity";


        public DeviceConfigAssetSecurityController(IDeviceConfigService<DeviceConfigAssetSecurityRequest, DeviceConfigAssetSecurityDetails> assetSecurityService,
            IInjectConfig injectConfig, ILoggingService loggingService) : base(injectConfig.ResolveKeyed<DeviceConfigRequestToAttributeMaps>("DeviceConfigRequestToAttributeMaps"), loggingService)
        {
            base._loggingService.CreateLogger(typeof(DeviceConfigAssetSecurityController));
            this._assetSecurityService = assetSecurityService;
        }

        [HttpPost]
        [Route("")]
        [UserUidParser]
        [ProducesResponseType(typeof(DeviceConfigAssetSecurityResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(DeviceConfigAssetSecurityResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(DeviceConfigAssetSecurityResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<DeviceConfigAssetSecurityResponse>> Fetch(DeviceConfigRequestBase deviceConfigBaseRequest)
        {

            deviceConfigBaseRequest = await base.ReadRequestContentAsync(deviceConfigBaseRequest);

            DeviceConfigAssetSecurityRequest deviceConfigAssetSecurityRequest = new DeviceConfigAssetSecurityRequest();
            
            if (deviceConfigBaseRequest == null)
            {
                throw new ArgumentNullException("deviceConfigBaseRequest is null");
            }
            else
            {
                deviceConfigAssetSecurityRequest.AssetUIDs = deviceConfigBaseRequest.AssetUIDs;
            }

            deviceConfigAssetSecurityRequest.ParameterGroupName = _parameterGroupName;
            deviceConfigAssetSecurityRequest.DeviceType = deviceConfigBaseRequest.DeviceType;
            deviceConfigAssetSecurityRequest.UserUID = base.GetUserContext(this.Request);
            deviceConfigAssetSecurityRequest.CustomerUID = base.GetCustomerContext(this.Request);

            deviceConfigAssetSecurityRequest.ConfigValues = new Dictionary<string, string>
            {
                {this._attributeMaps.Values["SecurityMode"], string.Empty},
                {this._attributeMaps.Values["SecurityStatus"], string.Empty}
            };

            this._loggingService.Info("Started invoking AssetSecurityService with request : " + JsonConvert.SerializeObject(deviceConfigBaseRequest), "DeviceConfigAssetSecurityController.Fetch");

            var response = await this._assetSecurityService.Fetch(deviceConfigAssetSecurityRequest);

            return base.SendResponse(HttpStatusCode.OK, new DeviceConfigAssetSecurityResponse(response.Lists, response.Errors.OfType<AssetErrorInfo>().ToList()));
            
        }

        [HttpPut]
        [Route("")]
        [UserUidParser]
        [ProducesResponseType(typeof(DeviceConfigAssetSecurityResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(DeviceConfigAssetSecurityResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(DeviceConfigAssetSecurityResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<DeviceConfigAssetSecurityResponse>> Save(DeviceConfigAssetSecurityRequest deviceConfigAssetSecurityRequest)
        {

            deviceConfigAssetSecurityRequest = await base.ReadRequestContentAsync(deviceConfigAssetSecurityRequest);

            if (deviceConfigAssetSecurityRequest == null)
            {
                throw new ArgumentNullException("deviceConfigAssetSecurityRequest is null");
            }

            deviceConfigAssetSecurityRequest.ParameterGroupName = _parameterGroupName;
            deviceConfigAssetSecurityRequest.UserUID = base.GetUserContext(Request);
            deviceConfigAssetSecurityRequest.CustomerUID = base.GetCustomerContext(Request);

            deviceConfigAssetSecurityRequest.ConfigValues = new Dictionary<string, string>();

            if (deviceConfigAssetSecurityRequest.SecurityMode.HasValue)
            {
                deviceConfigAssetSecurityRequest.ConfigValues.Add(this._attributeMaps.Values["SecurityMode"], deviceConfigAssetSecurityRequest.SecurityMode.ToString());
            }

            if (deviceConfigAssetSecurityRequest.SecurityStatus.HasValue)
            {
                deviceConfigAssetSecurityRequest.ConfigValues.Add(this._attributeMaps.Values["SecurityStatus"], ((int)deviceConfigAssetSecurityRequest.SecurityStatus.Value).ToString());
            }

            this._loggingService.Info("Started invoking AssetSecurityService with request : " + JsonConvert.SerializeObject(deviceConfigAssetSecurityRequest), "DeviceConfigAssetSecurityController.Save");

            var response = await this._assetSecurityService.Save(deviceConfigAssetSecurityRequest);

            return base.SendResponse(HttpStatusCode.OK, new DeviceConfigAssetSecurityResponse(response.Lists, response.Errors.OfType<AssetErrorInfo>().ToList()));
        }
    }
}
