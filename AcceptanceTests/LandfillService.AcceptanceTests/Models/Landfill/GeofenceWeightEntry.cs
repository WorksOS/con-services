using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandfillService.AcceptanceTests.Models.Landfill
{
    /// <summary>
    /// Weight entry submitted by the user for a geofence
    /// </summary>
    public class GeofenceWeightEntry
    {
        public DateTime date { get; set; }
        public bool entryPresent { get; set; }    // true if any site has a weight present
        public IEnumerable<GeofenceWeight> geofenceWeights { get; set; }
    }
}
