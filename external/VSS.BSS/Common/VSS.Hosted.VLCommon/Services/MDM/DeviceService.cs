using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using log4net;
using Newtonsoft.Json;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using VSS.Hosted.VLCommon.Services.MDM.Models;

namespace VSS.Hosted.VLCommon.Services.MDM
{
    public class DeviceService : ServiceBase, IDeviceService
    {
        private readonly ILog _log;

        private static readonly string DeviceApiBaseUri = ConfigurationManager.AppSettings["DeviceService.WebAPIURI"];

        public DeviceService()
        {
            _log = base.Logger;
        }

        public bool CreateDevice(object deviceDetails)
        {
            try
            {
                var stringified = JsonConvert.SerializeObject(deviceDetails, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });

                _log.IfDebugFormat("Creating a new device on the Next Gen VSP. Create Device Payload :{0}", stringified);
                var success = DispatchRequest(DeviceApiBaseUri,HttpMethod.Post, stringified);
                return success;
            }
            catch (Exception ex)
            {
                _log.IfWarnFormat("Error occurred while creating Device in VSP stack. Error message :{0}",
              ex.Message);
                return false;
            }
        }

        public bool UpdateDevice(object deviceDetails)
        {
            try
            {
                var stringified = JsonConvert.SerializeObject(deviceDetails, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
                _log.IfDebugFormat("Updating Device on the Next Gen VSP. Update Device Payload :{0}", stringified);
                var success = DispatchRequest(DeviceApiBaseUri, HttpMethod.Put, stringified);
                return success;
            }
            catch (Exception ex)
            {
                _log.IfWarnFormat("Error occurred while updating Device in VSP stack. Error message :{0}", ex.Message);
                return false;
            }
        }

        public bool AssociateDeviceAsset(AssociateDeviceAssetEvent associateAssetDeviceDetails)
        {
            try
            {
                var associateDeviceAssetUri = "/associatedeviceasset";
                var deviceDetails = JsonConvert.SerializeObject(associateAssetDeviceDetails, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include, DefaultValueHandling = DefaultValueHandling.Include });
                _log.IfDebugFormat("Associate Device Asset on the Next Gen VSP. Associate Device Asset Payload :{0}",
                deviceDetails);
                var success = DispatchRequest(DeviceApiBaseUri + associateDeviceAssetUri, HttpMethod.Post, deviceDetails);
                return success;
            }
            catch (Exception ex)
            {
                _log.IfWarnFormat("Error occurred while Associating Device and Asset in VSP stack. Error message :{0}", ex.Message);
                return false;
            }
        }

        public bool DissociateDeviceAsset(DissociateDeviceAssetEvent dissociateAssetDeviceDetails)
        {
            try
            {
                var dissociateDeviceAssetUri = "/dissociatedeviceasset";
                var deviceDetails = JsonConvert.SerializeObject(dissociateAssetDeviceDetails, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include, DefaultValueHandling = DefaultValueHandling.Include });
                _log.IfDebugFormat("Dissociate Device Asset on the Next Gen VSP. Dissociate Device Asset Payload :{0}",
                deviceDetails);
                var success = DispatchRequest(DeviceApiBaseUri + dissociateDeviceAssetUri, HttpMethod.Post, deviceDetails);
                return success;
            }
            catch (Exception ex)
            {
                _log.IfWarnFormat("Error occurred while Dissociating Device and Asset in VSP stack. Error message :{0}", ex.Message);
                return false;
            }
        }

        public bool ReplaceDevice(DeviceReplacementEvent replaceDeviceDetails)
        {
            try
            {
                var replaceDeviceAssetUri = "/devicereplacement";
                var deviceDetails = JsonConvert.SerializeObject(replaceDeviceDetails, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include, DefaultValueHandling = DefaultValueHandling.Include });
                _log.IfDebugFormat("Replace Device on the Next Gen VSP. Replace Device Payload :{0}",
                deviceDetails);
                var success = DispatchRequest(DeviceApiBaseUri + replaceDeviceAssetUri, HttpMethod.Post, deviceDetails);
                return success;
            }
            catch (Exception ex)
            {
                _log.IfWarnFormat("Error occurred while replacing Device in VSP stack. Error message :{0}", ex.Message);
                return false;
            }
        }
    }
}
