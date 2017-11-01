using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class CompactionVolumesSummaryResult : RequestResult, IEquatable<CompactionVolumesSummaryResult>
  {
    #region Members
    public CompactionVolumesSummaryData compactionVolumesSummaryData { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor: Success by default
    /// </summary>
    public CompactionVolumesSummaryResult()
      : base("success")
    { }
    #endregion

    #region Equality test
    public bool Equals(CompactionVolumesSummaryResult other)
    {
      if (other == null)
        return false;

      return this.compactionVolumesSummaryData.Equals(other.compactionVolumesSummaryData) &&
             this.Code == other.Code &&
             this.Message == other.Message;
    }

    public static bool operator ==(CompactionVolumesSummaryResult a, CompactionVolumesSummaryResult b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(CompactionVolumesSummaryResult a, CompactionVolumesSummaryResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is CompactionVolumesSummaryResult && this == (CompactionVolumesSummaryResult)obj;
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


    public class CompactionVolumesSummaryData : IEquatable<CompactionVolumesSummaryData>
    {
      public double netVolume { get; set; }

      public double totalVolume { get; set; }

      public double totalCutVolume { get; set; }

      public double totalFillVolume { get; set; }

      public double totalMachineCoveragePlanArea { get; set; }

      public double shrinkage { get; set; }
      public double bulking { get; set; }

      public bool Equals(CompactionVolumesSummaryData other)
      {
        if (other == null)
          return false;

        return Math.Round(this.bulking, 2) == Math.Round(other.bulking, 2) &&
               Math.Round(this.netVolume, 2) == Math.Round(other.netVolume, 2) &&
               Math.Round(this.shrinkage, 2) == Math.Round(other.shrinkage, 2) &&
               Math.Round(this.totalCutVolume, 2) == Math.Round(other.totalCutVolume, 2) &&
               Math.Round(this.totalFillVolume, 2) == Math.Round(other.totalFillVolume, 2) &&
               Math.Round(this.totalMachineCoveragePlanArea, 2) == Math.Round(other.totalMachineCoveragePlanArea, 2) &&
               Math.Round(this.totalVolume, 2) == Math.Round(other.totalVolume, 2) ;
      }

    }
  }
}
