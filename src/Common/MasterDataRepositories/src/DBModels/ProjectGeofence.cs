using System;

namespace VSS.MasterData.Repositories.DBModels
{
    public class ProjectGeofence
    {
        public string ProjectUID { get; set; }
        public string GeofenceUID { get; set; }
        public DateTime LastActionedUTC { get; set; }
        public GeofenceType? GeofenceType { get; set; }
    }
}