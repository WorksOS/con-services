using System;

namespace VSS.Productivity3D.Repo.DBModels
{
    public class ProjectGeofence
    {
        public string ProjectUID { get; set; }
        public string GeofenceUID { get; set; }
        public DateTime LastActionedUTC { get; set; }
    }
}