using log4net;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using VSS.Hosted.VLCommon.Services.MDM.Models;

namespace VSS.Hosted.VLCommon.Services.MDM
{
    public class SubscriptionService : ServiceBase, ISubscriptionService
    {
        private readonly ILog _log;
        private readonly HttpClient _httpClient;
        private static readonly string SubscriptionApiBaseUri = ConfigurationManager.AppSettings["SubscriptionService.WebAPIURI"];


        public SubscriptionService()
        {
            _log = base.Logger;
            _httpClient = new HttpClient
            {
                //Timeout = new TimeSpan(0, 0, AppConfigSettings.TimeOutValue)
            };
        }


        public bool CreateAssetSubscription(object assetSubscription)
        {
            try
            {
                var stringified = JsonConvert.SerializeObject(assetSubscription, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
                var success = DispatchRequest(SubscriptionApiBaseUri + "/asset", HttpMethod.Post, stringified);
                _log.IfDebugFormat("Create Asset Subscription Event Payload Data : {0} Posted Status : {1}", stringified, success ? "Success" : "Failed");
                return success;
            }
            catch (Exception ex)
            {
                _log.IfWarnFormat("Error occurred while creating Geofence in VSP stack. Error message :{0}",
                  ex.Message);
                return false;
            }
        }


        public bool UpdateAssetSubscription(object assetSubscription)
        {
            try
            {
                var stringified = JsonConvert.SerializeObject(assetSubscription, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
                var success = DispatchRequest(SubscriptionApiBaseUri + "/asset", HttpMethod.Put, stringified);
                _log.IfDebugFormat("Update Geofence Event Payload Data : {0} Posted Status : {1}", stringified, success ? "Success" : "Failed");
                return success;
            }
            catch (Exception ex)
            {
                _log.IfWarnFormat("Error occurred while updating Asset in VSP stack. Error message :{0}",
                ex.Message);
                return false;
            }
        }

        public bool CreateProjectSubscription(object projectSubscription)
        {
            try
            {
                var stringified = JsonConvert.SerializeObject(projectSubscription, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
                var success = DispatchRequest(SubscriptionApiBaseUri + "/project", HttpMethod.Post, stringified);
                _log.IfDebugFormat("Create Project Subscription Event Payload Data : {0} Posted Status : {1}", stringified, success ? "Success" : "Failed");
                return success;
            }
            catch (Exception ex)
            {
                _log.IfWarnFormat("Error occurred while creating Project in VSP stack. Error message :{0}",
                  ex.Message);
                return false;
            }
        }


        public bool UpdateProjectSubscription(object projectSubscription)
        {
            try
            {
                var stringified = JsonConvert.SerializeObject(projectSubscription, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
                var success = DispatchRequest(SubscriptionApiBaseUri + "/project", HttpMethod.Put, stringified);
                _log.IfDebugFormat("Update Project Subscription Event Payload Data : {0} Posted Status : {1}", stringified, success ? "Success" : "Failed");
                return success;
            }
            catch (Exception ex)
            {
                _log.IfWarnFormat("Error occurred while updating Project in VSP stack. Error message :{0}",
                ex.Message);
                return false;
            }
        }

        public bool CreateCustomerSubscription(object customerSubscription)
        {
          try
          {
            var stringified = JsonConvert.SerializeObject(customerSubscription, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
            var success = DispatchRequest(SubscriptionApiBaseUri + "/customer", HttpMethod.Post, stringified);
            _log.IfDebugFormat("Create Customer Subscription Event Payload Data : {0} Posted Status : {1}", stringified, success ? "Success" : "Failed");
            return success;
          }
          catch (Exception ex)
          {
            _log.IfWarnFormat("Error occurred while creating Customer in VSP stack. Error message :{0}",
              ex.Message);
            return false;
          }
        }

        public bool UpdateCustomerSubscription(object customerSubscription)
        {
          try
          {
            var stringified = JsonConvert.SerializeObject(customerSubscription, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
            var success = DispatchRequest(SubscriptionApiBaseUri + "/customer", HttpMethod.Put, stringified);
            _log.IfDebugFormat("Update Customer Subscription Event Payload Data : {0} Posted Status : {1}", stringified, success ? "Success" : "Failed");
            return success;
          }
          catch (Exception ex)
          {
            _log.IfWarnFormat("Error occurred while updating Customer in VSP stack. Error message :{0}",
            ex.Message);
            return false;
          }
        }

        public bool AssociateProjectSubscription(object association)
        {
          try
          {
            var stringified = JsonConvert.SerializeObject(association, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
            var success = DispatchRequest(SubscriptionApiBaseUri + "/project/AssociateProjectSubscription", HttpMethod.Post, stringified);
            _log.IfDebugFormat("Associate Project Subscription Event Payload Data : {0} Posted Status : {1}", stringified, success ? "Success" : "Failed");
            return success;
          }
          catch (Exception ex)
          {
            _log.IfWarnFormat("Error occurred while associating Project and subscription in VSP stack. Error message :{0}",
              ex.Message);
            return false;
          }
        }
    }
}
