using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using LandfillService.AcceptanceTests.Utils;

namespace LandfillService.AcceptanceTests.Models.Landfill
{
    /// <summary>
    /// Volume and time summary data 
    /// </summary>
    public class VolumeTime
    {
        public double currentWeekVolume { get; set; }
        public double currentMonthVolume { get; set; }
        public double? remainingVolume { get; set; }
        public double? remainingTime { get; set; }

        #region Equality test
        public bool Equals(VolumeTime other)
        {
            if (other == null)
                return false;

            return this.currentWeekVolume == other.currentWeekVolume &&
                this.currentMonthVolume == other.currentMonthVolume &&
                this.remainingVolume == other.remainingVolume &&
                this.remainingTime == other.remainingTime;
        }

        public static bool operator ==(VolumeTime a, VolumeTime b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(VolumeTime a, VolumeTime b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is VolumeTime && this == (VolumeTime)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

        #region ToString override
        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns>A string representation.</returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
        #endregion
    }
}
