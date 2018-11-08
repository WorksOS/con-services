using System;

namespace VSS.MasterData.Models.Models
{
    public class AssociateProjectGeofenceData
    {
        public Guid GeofenceUID { get; set; }
        public Guid ProjectUID { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime ActionUTC => DateTime.UtcNow;
    }
}