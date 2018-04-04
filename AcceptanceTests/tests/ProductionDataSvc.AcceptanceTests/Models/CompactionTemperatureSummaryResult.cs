using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;
using System;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class CompactionTemperatureSummaryResult : RequestResult, IEquatable<CompactionTemperatureSummaryResult>
  {
    #region Members
    /// <summary>
    /// The Temperature summary data results
    /// </summary>
    public TemperatureSummaryData temperatureSummaryData { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor: Success by default
    /// </summary>
    public CompactionTemperatureSummaryResult()
            : base("success")
    { }
    #endregion

    #region Equality test
    public bool Equals(CompactionTemperatureSummaryResult other)
    {
      if (other == null)
        return false;

      if (this.temperatureSummaryData == null)
      {
        return this.Code == other.Code &&
               this.Message == other.Message;
      }

      return this.temperatureSummaryData.Equals(other.temperatureSummaryData) &&
       this.Code == other.Code &&
       this.Message == other.Message;
    }

    public static bool operator ==(CompactionTemperatureSummaryResult a, CompactionTemperatureSummaryResult b)
    {
      if ((object)a == null || (object)b == null)
        return Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(CompactionTemperatureSummaryResult a, CompactionTemperatureSummaryResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is CompactionTemperatureSummaryResult && this == (CompactionTemperatureSummaryResult)obj;
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

    /// <summary>
    /// Temperature summary data returned
    /// </summary>
    public class TemperatureSummaryData : IEquatable<TemperatureSummaryData>
    {
      /// <summary>
      /// The percentage of cells that are compacted within the target bounds
      /// </summary>
      public double percentEqualsTarget { get; set; }
      /// <summary>
      /// The percentage of the cells that are over-compacted
      /// </summary>
      public double percentGreaterThanTarget { get; set; }
      /// <summary>
      /// The percentage of the cells that are under compacted
      /// </summary>
      public double percentLessThanTarget { get; set; }
      /// <summary>
      /// The total area covered by non-null cells in the request area
      /// </summary>
      public double totalAreaCoveredSqMeters { get; set; }
      /// <summary>
      /// Temperature machine target and whether it is constant or varies.
      /// </summary>
      public TemperatureTargetData temperatureTarget { get; set; }


      public bool Equals(TemperatureSummaryData other)
      {
        if (other == null)
        {
          return false;
        }

        var tempEquals = this.temperatureTarget?.Equals(other.temperatureTarget) ?? true;

        return Math.Round(this.percentEqualsTarget, 2) == Math.Round(other.percentEqualsTarget, 2) &&
               Math.Round(this.percentGreaterThanTarget, 2) == Math.Round(other.percentGreaterThanTarget, 2) &&
               Math.Round(this.percentLessThanTarget, 2) == Math.Round(other.percentLessThanTarget, 2) &&
               Math.Round(this.totalAreaCoveredSqMeters, 2) == Math.Round(other.totalAreaCoveredSqMeters, 2) &&
               tempEquals;
      }
    }

    /// <summary>
    /// Temperature target data returned
    /// </summary>
    public class TemperatureTargetData : IEquatable<TemperatureTargetData>
    {
      /// <summary>
      /// If the Temperature value is constant, this is the minimum constant value of all Temperature targets in the processed data.
      /// </summary>
      public double minTemperatureMachineTarget { get; set; }
      /// <summary>
      /// If the Temperature value is constant, this is the maximum constant value of all Temperature targets in the processed data.
      /// </summary>
      public double maxTemperatureMachineTarget { get; set; }
      /// <summary>
      /// Are the Temperature target values applying to all processed cells varying?
      /// </summary>
      public bool targetVaries { get; set; }

      public bool Equals(TemperatureTargetData other)
      {
        if (other == null)
          return false;

        return this.minTemperatureMachineTarget == other.minTemperatureMachineTarget &&
               this.maxTemperatureMachineTarget == other.maxTemperatureMachineTarget &&
               this.targetVaries == other.targetVaries;
      }
    }
  }
}
