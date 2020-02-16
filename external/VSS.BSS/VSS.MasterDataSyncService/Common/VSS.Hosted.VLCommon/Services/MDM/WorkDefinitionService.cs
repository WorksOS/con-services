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
    public class WorkDefinitionService : ServiceBase, IWorkDefinitionService
    {
        private readonly ILog _log;

        private static readonly string WorkDefinitionApiBaseUri = ConfigurationManager.AppSettings["AssetService.WebAPIURI"] + "/workdefinition";

        public WorkDefinitionService()
        {
            _log = base.Logger;
        }

        public bool CreateWorkDefinition(object workDefinitionDetails)
        {
            try
            {
                var stringified = JsonConvert.SerializeObject(workDefinitionDetails, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });

                _log.IfDebugFormat("Creating a new Work Definition on the Next Gen VSP. Create Work Definition Payload :{0}",
                   stringified);

                var success = DispatchRequest(WorkDefinitionApiBaseUri, HttpMethod.Post, stringified);
                return success;
            }
            catch (Exception ex)
            {
                _log.IfWarnFormat("Error occurred while creating Work Definition in VSP stack. Error message :{0}",
              ex.Message);
                return false;
            }
        }

        public bool UpdateWorkDefinition(object workDefinitionDetails)
        {
            try
            {
                var stringified = JsonConvert.SerializeObject(workDefinitionDetails, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
                _log.IfDebugFormat("Updating Work Definition on the Next Gen VSP. Update Work Definition Payload :{0}",
                stringified);
                var success = DispatchRequest(WorkDefinitionApiBaseUri, HttpMethod.Put, stringified);
                return success;
            }
            catch (Exception ex)
            {
                _log.IfWarnFormat("Error occurred while updating Work Definition  in VSP stack. Error message :{0}",
                        ex.Message);
                return false;
            }
        }
    }
}
