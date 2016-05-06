using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandfillService.AcceptanceTests.Models.Landfill
{
    /// <summary>
    /// Weight entry for a geofence
    /// </summary>
    public class GeofenceWeight
    {
        public Guid geofenceUid { get; set; }
        public double weight { get; set; }
    }
}
