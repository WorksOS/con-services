using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandfillService.AcceptanceTests.Models.Landfill
{
    /// <summary>
    /// An entry for CCA for a machine 
    /// </summary>
    public class CCAEntry
    {
        public DateTime date { get; set; }
        public double ccaPercent { get; set; }
    }

    /// <summary>
    /// CCA% representation for a machine
    /// </summary>
    public class CCAData
    {
        public string machineName { get; set; }
        public IEnumerable<CCAEntry> entries { get; set; }
    }
}
