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
    public class GeofenceService : ServiceBase, IGeofenceService
    {
        private readonly ILog _log;
        private readonly HttpClient _httpClient;
        private static readonly string GeofenceApiBaseUri = ConfigurationManager.AppSettings["GeofenceService.WebAPIURI"];

        public GeofenceService()
        {
            _log = base.Logger;
            _httpClient = new HttpClient
            {
                //Timeout = new TimeSpan(0, 0, AppConfigSettings.TimeOutValue)
            };
        }


        public bool CreateGeofence(object geofenceDetails)
        {
            try
            {
               var stringified = JsonConvert.SerializeObject(geofenceDetails, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
               var success = DispatchRequest(GeofenceApiBaseUri, HttpMethod.Post, stringified);
               _log.IfDebugFormat("Create Geofence Event Payload Data : {0} Posted Status : {1}", stringified, success ? "Success" : "Failed" );
               return success;
            }
            catch (Exception ex)
            {
                _log.IfWarnFormat("Error occurred while creating Geofence in VSP stack. Error message :{0}",
                                ex.Message);
                return false;
            }
        }


        public bool UpdateGeofence(object geofenceDetails)
        {
            try
            {
                var updateEvent = JsonConvert.SerializeObject(geofenceDetails, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
                var success = DispatchRequest(GeofenceApiBaseUri, HttpMethod.Put, updateEvent);
                _log.IfDebugFormat("Update Geofence Event Payload Data : {0} Posted Status : {1}", updateEvent, success ? "Success" : "Failed");
                return success;
            }
            catch (Exception ex)
            {
                _log.IfWarnFormat("Error occurred while updating Geofence in VSP stack. Error message :{0}",
                        ex.Message);
                return false;
            }
        }

        public bool DeleteGeofence(string geofenceGuid, string userGuid)
        {
            try
            {
                var deleteGeofenceApiUrl = String.Format("{0}?geofenceuid={1}&useruid={2}&actionutc={3}", GeofenceApiBaseUri, geofenceGuid, userGuid, DateTime.UtcNow);
                var success = DispatchRequest(deleteGeofenceApiUrl, HttpMethod.Delete);
                _log.IfDebugFormat("Delete Geofence Event Payload Data : {0} Posted Status : {1}", geofenceGuid, success ? "Success" : "Failed");
                return success;
            }
            catch (Exception ex)
            {
                _log.IfWarnFormat("Error occurred while deleting Geofence in VSP stack. Error message :{0}",
                        ex.Message);
                return false;
            }
        }
        public bool FavoriteGeofence(string geofenceGuid, string userGuid, string customerGuid)
        {
            try
            {
                var favoriteGeofenceApiUrl = String.Format("{0}/markfavorite?useruid={1}&customeruid={2}&geofenceuid={3}&actionutc={4}", GeofenceApiBaseUri, userGuid, customerGuid, geofenceGuid, DateTime.UtcNow);
                var success = DispatchRequest(favoriteGeofenceApiUrl, HttpMethod.Put);
                _log.IfDebugFormat("Favorite Geofence Event Payload Data : {0} Posted Status : {1}", geofenceGuid, success ? "Success" : "Failed");
                return success;
            }
            catch (Exception ex)
            {
                _log.IfWarnFormat("Error occurred during the Favorite Geofence Event in VSP stack. Error message :{0}",
                        ex.Message);
                return false;
            }
        }

        public bool UnfavoriteGeofence(string geofenceGuid, string userGuid, string customerGuid)
        {
            try
            {
                var favoriteGeofenceApiUrl = String.Format("{0}/markunfavorite?useruid={1}&customeruid={2}&geofenceuid={3}&actionutc={4}", GeofenceApiBaseUri, userGuid, customerGuid, geofenceGuid, DateTime.UtcNow);
                var success = DispatchRequest(favoriteGeofenceApiUrl, HttpMethod.Put);
                _log.IfDebugFormat("Favorite Geofence Event Payload Data : {0} Posted Status : {1}", geofenceGuid, success ? "Success" : "Failed");
                return success;
            }
            catch (Exception ex)
            {
                _log.IfWarnFormat("Error occurred during the Unfavorite Geofence Event in VSP stack. Error message :{0}",
                        ex.Message);
                return false;
            }
        }
    }
}
