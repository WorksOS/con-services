using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandfillService.AcceptanceTests.Models.Landfill
{
    /// <summary>
    /// A CCA ratio for a machine for one day (for all lifts)
    /// </summary>
    public class CCARatioEntry
    {
        public DateTime date { get; set; }
        public double ccaRatio { get; set; }
    }

    /// <summary>
    /// CCA ratio representation for a machine
    /// </summary>
    public class CCARatioData
    {
        public string machineName { get; set; }
        public IEnumerable<CCARatioEntry> entries { get; set; }
    }

    /// <summary>
    /// CCA summary representation for a machine
    /// </summary>
    public class CCASummaryData
    {
        public string machineName { get; set; }
        public int? liftId { get; set; }
        public double incomplete { get; set; }
        public double complete { get; set; }
        public double overcomplete { get; set; }
    }

    /// <summary>
    /// Represents a CCA entry from the database
    /// </summary>
    public class CCA
    {
        public string geofenceUid { get; set; }
        public DateTime date { get; set; }
        public long machineId { get; set; }
        public int? liftId { get; set; }
        public double incomplete { get; set; }
        public double complete { get; set; }
        public double overcomplete { get; set; }
    }
}
