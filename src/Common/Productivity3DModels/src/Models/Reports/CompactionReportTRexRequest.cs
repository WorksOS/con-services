using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Exceptions;
using VSS.Productivity3D.Models.Validation;

namespace VSS.Productivity3D.Models.Models.Reports
{

  /// <summary>
  /// The request representation for getting production data from TRex for a grid or stationOffset report.
  /// </summary>
  /// 
  public abstract class CompactionReportTRexRequest 
  {
    /// <summary>
    /// A project unique identifier.
    /// </summary>
    [JsonProperty(PropertyName = "projectUid", Required = Required.Default)]
    [ValidProjectUID]
    public Guid ProjectUid { get; set; }

    /// <summary>
    /// The filter instance to use in the request
    /// Value may be null.
    /// </summary>
    /// 
    [JsonProperty(Required = Required.Default)]
    public FilterResult Filter { get; protected set; }

    /// <summary>
    /// Include the measured elevation at the sampled location
    /// </summary>
    /// 
    [JsonProperty(Required = Required.Default)]
    public bool ReportElevation { get; protected set; }

    /// <summary>
    /// Include the measured CMV at the sampled location
    /// </summary
    /// >
    [JsonProperty(Required = Required.Default)]
    public bool ReportCmv { get; protected set; }

    /// <summary>
    /// Include the measured MDP at the sampled location
    /// </summary>
    /// 
    [JsonProperty(Required = Required.Default)]
    public bool ReportMdp { get; protected set; }

    /// <summary>
    /// Include the calculated pass count at the sampled location
    /// </summary>
    /// 
    [JsonProperty(Required = Required.Default)]
    public bool ReportPassCount { get; protected set; }

    /// <summary>
    /// Include the measured temperature at the sampled location
    /// </summary>
    /// 
    [JsonProperty(Required = Required.Default)]
    public bool ReportTemperature { get; protected set; }

    /// <summary>
    /// Include the calculated cut-fill between the elevation at the sampled location and the design elevation at the same location
    /// </summary>
    /// 
    [JsonProperty(Required = Required.Default)]
    public bool ReportCutFill { get; protected set; }

    /// <summary>
    /// Sets the design file to be used for cut/fill calculations
    /// </summary>
    /// 
    [JsonProperty(Required = Required.Default)]
    public Guid? CutFillDesignUid { get; protected set; }

    /// <summary>
    /// Sets the design offset to be used for cut/fill calculations
    /// </summary>
    /// 
    [JsonProperty(Required = Required.Default)]
    public double CutFillDesignOffset { get; protected set; }

    /// <summary>
    /// Validates properties.
    /// </summary>
    /// 
    public virtual void Validate()
    {
      if (ReportCutFill)
      {
        if (CutFillDesignUid == null || CutFillDesignUid.Value == Guid.Empty)
        {
          throw new MissingDesignDescriptorException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              string.Format(
                "Design descriptor required for cut/fill and design to filter or filter to design volumes display")));
        }
      }

      if (!(ReportPassCount || ReportTemperature || ReportMdp || ReportCutFill || ReportCmv || ReportElevation))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "There are no selected fields to be reported on"));
      }
    }
  }
}
