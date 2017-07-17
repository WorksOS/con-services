using System;
using Newtonsoft.Json;

namespace VSS.MasterData.Models.Models
{
    /// <summary>
    /// Describes geofence data returned by the geofence master data service.
    /// </summary>
    public class GeofenceData : IData
    {
        public string GeofenceName { get; set; }

        public string Description { get; set; }

        public string GeofenceType { get; set; }

        public string GeometryWKT { get; set; }

        public int FillColor { get; set; }

        public bool IsTransparent { get; set; }

        public Guid CustomerUID { get; set; }

        public Guid GeofenceUID { get; set; }

        public Guid UserUID { get; set; }

        public DateTime ActionUTC => DateTime.UtcNow;

        /// <summary>
        /// Key to use for caching geofence master data
        /// </summary>
        [JsonIgnore]
        public string CacheKey => GeofenceUID.ToString();
    }
}
