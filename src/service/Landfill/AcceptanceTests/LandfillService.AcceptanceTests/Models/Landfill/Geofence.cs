using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandfillService.AcceptanceTests.Models.Landfill
{
    /// <summary>
    /// Geofence representation
    /// </summary>
    public class Geofence
    {
        public Guid uid { get; set; }
        public string name { get; set; }
        public int type { get; set; }
    }
}
