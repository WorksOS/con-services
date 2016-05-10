using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandfillService.AcceptanceTests.Models.Landfill
{
    /// <summary>
    /// Encapsulates weight data sent to the client  
    /// </summary>
    public class WeightData
    {
        public IEnumerable<GeofenceWeightEntry> entries { get; set; }
        public bool retrievingVolumes { get; set; }          // is the service currently retrieving volumes for this project?
        public Project project { get; set; }
    }
}
