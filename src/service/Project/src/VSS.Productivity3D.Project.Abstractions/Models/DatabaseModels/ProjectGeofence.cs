using System;
using VSS.MasterData.Repositories.DBModels;

namespace VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels
{
    public class ProjectGeofence
    {
        public string ProjectUID { get; set; }
        public string GeofenceUID { get; set; }
        public DateTime LastActionedUTC { get; set; }
        public GeofenceType? GeofenceType { get; set; }
    }
}