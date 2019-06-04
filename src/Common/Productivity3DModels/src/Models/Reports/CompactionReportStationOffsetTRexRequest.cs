using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Utilities;

namespace VSS.Productivity3D.Models.Models.Reports
{
  /// <summary>
  /// The request representation for getting production data from Raptor for a grid report.
  /// </summary>
  public class CompactionReportStationOffsetTRexRequest : CompactionReportTRexRequest
  {
    /// <summary>
    /// Sets the alignment file to be used for station/offset calculations
    /// </summary>
    /// 
    [JsonProperty(Required = Required.Always)]
    public Guid AlignmentDesignUid { get; protected set; }

    /// <summary>
    /// The spacing interval for the sampled points. Setting to 1.0 will cause points to be spaced 1.0 meters apart.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public double CrossSectionInterval { get; protected set; }

    [JsonProperty(Required = Required.Always)]
    public double StartStation { get; set; }

    [JsonProperty(Required = Required.Always)]
    public double EndStation { get; set; }

    [JsonProperty(Required = Required.Always)]
    public double[] Offsets { get; set; }

    protected CompactionReportStationOffsetTRexRequest()
    { }

    public static CompactionReportStationOffsetTRexRequest CreateRequest(
      Guid projectUid,
      FilterResult filter,
      bool reportElevation,
      bool reportCmv,
      bool reportMdp,
      bool reportPassCount,
      bool reportTemperature,
      bool reportCutFill,
      Guid? cutFillDesignUid,
      double? cutFillDesignOffset,
      Guid alignmentDesignUid,
      double crossSectionInterval,
      double startStation,
      double endStation,
      double[] offsets)
    {
      return new CompactionReportStationOffsetTRexRequest
      {
        ProjectUid = projectUid,
        Filter = filter,
        ReportElevation = reportElevation,
        ReportCmv = reportCmv,
        ReportMdp = reportMdp,
        ReportPassCount = reportPassCount,
        ReportTemperature = reportTemperature,
        ReportCutFill = reportCutFill,
        CutFillDesignUid = cutFillDesignUid,
        CutFillDesignOffset = cutFillDesignOffset,
        AlignmentDesignUid = alignmentDesignUid,
        CrossSectionInterval = crossSectionInterval,
        StartStation = startStation,
        EndStation = endStation,
        Offsets = offsets
      };
    }

    /// <summary>
    /// Validates properties.
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      if (AlignmentDesignUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Alignment file must be specified for station and offset report."));
      }

      if (this.CrossSectionInterval < ValidationConstants3D.MIN_SPACING_INTERVAL || this.CrossSectionInterval > ValidationConstants3D.MAX_SPACING_INTERVAL)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Interval must be >= {ValidationConstants3D.MIN_SPACING_INTERVAL}m and <= {ValidationConstants3D.MAX_SPACING_INTERVAL}m. Actual value: {this.CrossSectionInterval}"));
      }

      if (Offsets == null || Offsets.Length == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Offsets must be specified for station and offset report."));
      }

      // Start and end stations negative value are allowed...
      if (StartStation > EndStation)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid station range for station and offset report."));
      }
    }
  }
}
