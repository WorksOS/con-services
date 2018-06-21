using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.Report.Models
{
  /// <summary>
  /// Represents typics request for summary reports. Currently include Summary Volumes and Summary Thickness.
  /// </summary>
  public class SummaryParametersBase : ProjectID
  {
    /// <summary>
    /// An identifying string from the caller
    /// </summary>
    [JsonProperty(PropertyName = "callId", Required = Required.Default)]
    public Guid? CallId { get; protected set; }

    /// <summary>
    /// The base or earliest filter to be used for filter-filter and filter-design volumes.
    /// </summary>
    [JsonProperty(PropertyName = "baseFilter", Required = Required.Default)]
    public FilterResult BaseFilter { get; protected set; }

    /// <summary>
    /// The ID of the base or earliest filter to be used for filter-filter and filter-design volumes.
    /// </summary>
    [JsonProperty(PropertyName = "baseFilterID", Required = Required.Default)]
    public long BaseFilterId { get; protected set; }

    /// <summary>
    /// The top or latest filter to be used for filter-filter and design-filter volumes
    /// </summary>
    [JsonProperty(PropertyName = "topFilter", Required = Required.Default)]
    public FilterResult TopFilter { get; protected set; }

    /// <summary>
    /// The ID of the top or latest filter to be used for filter-filter and design-filter volumes
    /// </summary>
    [JsonProperty(PropertyName = "topFilterID", Required = Required.Default)]
    public long TopFilterId { get; protected set; }

    /// <summary>
    /// An additional spatial constraining filter that may be used to provide additional control over the area the summary volumes are being calculated over.
    /// </summary>
    [JsonProperty(PropertyName = "additionalSpatialFilter", Required = Required.Default)]
    public FilterResult AdditionalSpatialFilter { get; protected set; }

    /// <summary>
    /// The ID of an additional spatial constraining filter that may be used to provide additional control over the area the summary volumes are being calculated over.
    /// </summary>
    [JsonProperty(PropertyName = "additionalSpatialFilterID", Required = Required.Default)]
    public long AdditionalSpatialFilterId { get; protected set; }

    /// <summary>
    /// The set of parameters and configuration information relevant to analysis of compaction material layers information for related profile queries.
    /// </summary>
    [JsonProperty(PropertyName = "liftBuildSettings", Required = Required.Default)]
    public LiftBuildSettings LiftBuildSettings { get; protected set; }

    public override void Validate()
    {
      base.Validate(); 

      if (this.LiftBuildSettings.liftThicknessTarget == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Target thickness must be specified for the request."));
      }

      this.LiftBuildSettings?.Validate();
      this.AdditionalSpatialFilter?.Validate();
      this.TopFilter?.Validate();
      this.BaseFilter?.Validate();
    }

    /// <summary>
    /// Prevents a default instance of the <see cref="SummaryParametersBase"/> class from being created.
    /// </summary>
    protected SummaryParametersBase()
    { }
  }
}
