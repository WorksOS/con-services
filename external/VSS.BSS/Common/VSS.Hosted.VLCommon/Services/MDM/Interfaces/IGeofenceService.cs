
using VSS.Hosted.VLCommon.Services.MDM.Models;

namespace VSS.Hosted.VLCommon.Services.MDM.Interfaces
{
    public interface IGeofenceService
    {
        bool CreateGeofence(object geofenceDetails);
        bool UpdateGeofence(object geofenceDetails);
        bool DeleteGeofence(string geofenceGuid, string userGuid);
        bool FavoriteGeofence(string geofenceGuid, string userGuid, string customerGuid);
        bool UnfavoriteGeofence(string geofenceGuid, string userGuid, string customerGuid);
        bool PurgePublish(object geofencePurge);
    }
}
