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
    /// A CCA ratio for a machine for one day (for all lifts)
    /// </summary>
    public class CCARatioEntry
    {
        public DateTime date { get; set; }
        public double ccaRatio { get; set; }

        #region Equality test
        public bool Equals(CCARatioEntry other)
        {
            if (other == null)
                return false;

            return this.date == other.date && Math.Round(this.ccaRatio, 1) == Math.Round(other.ccaRatio, 1);
        }

        public static bool operator ==(CCARatioEntry a, CCARatioEntry b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(CCARatioEntry a, CCARatioEntry b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is CCARatioEntry && this == (CCARatioEntry)obj;
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

    /// <summary>
    /// CCA ratio representation for a machine
    /// </summary>
    public class CCARatioData
    {
        public string machineName { get; set; }
        public List<CCARatioEntry> entries { get; set; }

        #region Equality test
        public bool Equals(CCARatioData other)
        {
            if (other == null)
                return false;

            return this.machineName == other.machineName && LandfillCommonUtils.ListsAreEqual<CCARatioEntry>(this.entries, other.entries);
        }

        public static bool operator ==(CCARatioData a, CCARatioData b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(CCARatioData a, CCARatioData b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is CCARatioData && this == (CCARatioData)obj;
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

        #region Equality test
        public bool Equals(CCASummaryData other)
        {
            if (other == null)
                return false;

            return this.machineName == other.machineName &&
                this.liftId == other.liftId &&
                Math.Round(this.incomplete, 1) == Math.Round(other.incomplete, 1) &&
                Math.Round(this.complete, 1) == Math.Round(other.complete, 1) &&
                Math.Round(this.overcomplete, 1) == Math.Round(other.overcomplete, 1);
        }

        public static bool operator ==(CCASummaryData a, CCASummaryData b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(CCASummaryData a, CCASummaryData b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is CCASummaryData && this == (CCASummaryData)obj;
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
