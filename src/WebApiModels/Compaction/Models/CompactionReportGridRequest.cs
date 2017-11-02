using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.Utilities;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  /// <summary>
  /// The request representation for getting production data from Raptor for a grid report.
  /// </summary>
  /// 
  public class CompactionReportGridRequest : CompactionReportRequest
  {
    /// <summary>
    /// The spacing interval for the sampled points. Setting to 1.0 will cause points to be spaced 1.0 meters apart.
    /// </summary>
    /// 
    [JsonProperty(Required = Required.Always)]
    public double GridInterval { get; protected set; }

    /// <summary>
    /// Grid report option. Whether it is defined automatically or by user specified parameters.
    /// </summary>
    /// 
    [JsonProperty(Required = Required.Default)]
    public GridReportOption GridReportOption { get; protected set; }

    /// <summary>
    /// The Northing ordinate of the location to start gridding from
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public double StartNorthing { get; protected set; }

    /// <summary>
    /// The Easting ordinate of the location to start gridding from
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public double StartEasting { get; protected set; }

    /// <summary>
    /// The Northing ordinate of the location to end gridding at
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public double EndNorthing { get; protected set; }

    /// <summary>
    /// The Easting ordinate of the location to end gridding at
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public double EndEasting { get; protected set; }

    /// <summary>
    /// The orientation of the grid, expressed in radians
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public double Azimuth { get; protected set; }

    public static CompactionReportGridRequest CreateCompactionReportGridRequest(
      long projectId, 
      Filter filter, 
      long filterId,
      LiftBuildSettings liftBuildSettings,
      bool reportElevation,
      bool reportCMV,
      bool reportMDP,
      bool reportPassCount,
      bool reportTemperature,
      bool reportCutFill,
      DesignDescriptor designFile,
      double? gridInerval,
      GridReportOption gridReportOption,
      double startNorthing,
      double startEasting,
      double endNorthing,
      double endEasting,
      double azimuth)
    {
      return new CompactionReportGridRequest()
      {
        projectId = projectId,
        Filter = filter,
        FilterID = filterId,
        LiftBuildSettings = liftBuildSettings,
        ReportElevation = reportElevation,
        ReportCMV = reportCMV,
        ReportMDP = reportMDP,
        ReportPassCount = reportPassCount,
        ReportTemperature = reportTemperature,
        ReportCutFill = reportCutFill,
        DesignFile = designFile,
        GridInterval = gridInerval ?? ValidationConstants.DEFAULT_SPACING_INERVAL,
        GridReportOption = gridReportOption,
        StartNorthing = startNorthing,
        StartEasting = startEasting,
        EndNorthing = endNorthing,
        EndEasting = endEasting,
        Azimuth = azimuth
      };
    }

    /// <summary>
    /// Validates properties.
    /// </summary>
    /// 
    public override void Validate()
    {
      base.Validate();

      if (!(GridReportOption == GridReportOption.Automatic ||
            GridReportOption == GridReportOption.Direction ||
            GridReportOption == GridReportOption.EndPoint))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Report option for gridded report type must be 1 ('Automatic'), 2 ('Direction') or 3 (EndPoint). Actual value supplied: {GridReportOption}"));
      }

      if (GridInterval < ValidationConstants.MIN_SPACING_INTERVAL || GridInterval > ValidationConstants.MAX_SPACING_INTERVAL)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Interval must be >= 0.1m and <= 100.0m. Actual value: {GridInterval}"));
      }

      if (Azimuth < 0 || Azimuth > (2 * Math.PI))
      {
        if (!(ReportPassCount || ReportTemperature || ReportMDP || ReportCutFill || ReportCMV || ReportElevation))
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              $"Azimuth must be in the range 0..2*PI radians. Actual value: {Azimuth}"));
        }
      }
    }
    
  }

  /// <summary>
  /// 
  /// </summary>
  /// 
  public enum CompactionReportType
  {
    Grid = 1,
    StationOffset = 2,
  }

  /// <summary>
  /// Grid report option enum.
  /// </summary>
  /// 
  public enum GridReportOption
  {
    Direction = 0,
    EndPoint = 1,
    Automatic = 2
  }
}
