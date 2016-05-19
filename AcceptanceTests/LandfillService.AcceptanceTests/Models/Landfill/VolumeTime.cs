using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandfillService.AcceptanceTests.Models.Landfill
{
    /// <summary>
    /// Volume and time sumamry data 
    /// </summary>
    public class VolumeTime
    {
        public double currentWeekVolume { get; set; }
        public double currentMonthVolume { get; set; }
        public double remainingVolume { get; set; }
        public double remainingTime { get; set; }
    }
}
