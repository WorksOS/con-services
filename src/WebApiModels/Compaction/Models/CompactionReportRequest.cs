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
  public abstract class CompactionReportRequest : RaptorHelper
  {
    /// <summary>
    /// The filter instance to use in the request
    /// Value may be null.
    /// </summary>
    /// 
    [JsonProperty(Required = Required.Default)]
    public Filter Filter { get; protected set; }

    /// <summary>
    /// The filter ID to be used in the request.
    /// May be null.
    /// </summary>
    /// 
    [JsonProperty(Required = Required.Default)]
    public long FilterID { get; protected set; }

    /// <summary>
    /// A collection of parameters and configuration information relating to analysis and determination of material layers.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public LiftBuildSettings LiftBuildSettings { get; protected set; }

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
    public bool ReportCMV { get; protected set; }

    /// <summary>
    /// Include the measured MDP at the sampled location
    /// </summary>
    /// 
    [JsonProperty(Required = Required.Default)]
    public bool ReportMDP { get; protected set; }

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
    /// Sets the design file to be used for cut/fill or station/offset calculations
    /// </summary>
    /// 
    [JsonProperty(Required = Required.Default)]
    public DesignDescriptor DesignFile { get; protected set; }

    /// <summary>
    /// Validates properties.
    /// </summary>
    /// 
    public override void Validate()
    {
      base.Validate();

      // Compaction settings
      if (LiftBuildSettings != null)
      {
        LiftBuildSettings.Validate();
      }
      
      if (ReportCutFill)
      {
        ValidateDesign(DesignFile, DisplayMode.CutFill, RaptorConverters.VolumesType.None);
      }

      if (!(ReportPassCount || ReportTemperature || ReportMDP || ReportCutFill || ReportCMV || ReportElevation))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "There are no selected fields to be reported on"));
      }
    }
  }
}
