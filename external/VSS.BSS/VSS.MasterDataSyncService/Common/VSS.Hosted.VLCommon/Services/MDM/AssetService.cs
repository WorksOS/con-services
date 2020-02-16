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
    public class AssetService : ServiceBase, IAssetService
    {
        private readonly ILog _log;

        private static readonly string AssetApiBaseUri = ConfigurationManager.AppSettings["AssetService.WebAPIURI"] + "/asset";

        public AssetService()
        {
            _log = base.Logger;
        }

        public bool CreateAsset(object assetDetails)
        {
            try
            {
                var stringified = JsonConvert.SerializeObject(assetDetails, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                _log.IfDebugFormat("Creating a new Asset on the Next Gen VSP. Create Asset Payload :{0}", stringified);
                var success = DispatchRequest(AssetApiBaseUri, HttpMethod.Post, stringified);
                return success;
            }
            catch (Exception ex)
            {
                _log.IfWarnFormat("Error occurred while creating Asset in VSP stack. Error message :{0}",
              ex.Message);
                return false;
            }
        }

        public bool UpdateAsset(object assetDetails)
        {
            try
            {
                var stringified = JsonConvert.SerializeObject(assetDetails, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
                _log.IfDebugFormat("Updating Asset on the Next Gen VSP. Update Asset Payload :{0}", stringified);
                var success = DispatchRequest(AssetApiBaseUri, HttpMethod.Put, stringified);
                return success;
            }
            catch (Exception ex)
            {
                _log.IfWarnFormat("Error occurred while updating Asset in VSP stack. Error message :{0}",
                        ex.Message);
                return false;
            }
        }
    }
}
