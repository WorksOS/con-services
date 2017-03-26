using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.ResultHandling;

namespace VSS.Raptor.Service.WebApiModels.Report.Models
{
  /// <summary>
  /// Represents speed summary request.
  /// </summary>
  public class SummarySpeedRequest : ProjectID, IValidatable
  {

    /// <summary>
    /// An identifying string from the caller
    /// </summary>
    [JsonProperty(PropertyName = "callId", Required = Required.Default)]
    public Guid? callId { get; protected set; }

    /// <summary>
    /// The filter to be used 
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public Filter filter { get; protected set; }

    /// <summary>
    /// Gets or sets the filter identifier.
    /// </summary>
    /// <value>
    /// The filter identifier.
    /// </value>
    [JsonProperty(PropertyName = "filterId", Required = Required.Default)]
    public int filterId { get; protected set; }

    /// <summary>
    /// The set of parameters and configuration information relevant to analysis of compaction material layers information for related profile queries.
    /// </summary>
    [JsonProperty(PropertyName = "liftBuildSettings", Required = Required.Default)]
    public LiftBuildSettings liftBuildSettings { get; protected set; }

    public override void Validate()
    {
      base.Validate();
      if (liftBuildSettings.machineSpeedTarget == null)
        throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Target speed must be specified for the request."));

      if (this.liftBuildSettings != null)
        this.liftBuildSettings.Validate();
    }

    /// <summary>
    /// Prevents a default instance of the <see cref="SummaryParametersBase"/> class from being created.
    /// </summary>
    protected SummarySpeedRequest()
    {
    }

    /// <summary>
    /// Create instance of SummarySpeedRequest
    /// </summary>
    public static SummarySpeedRequest CreateSummarySpeedRequestt(
      long projectID,
      Guid? callId,
      LiftBuildSettings liftBuildSettings,
      Filter filter,
      int filterID
        )
    {
      return new SummarySpeedRequest
      {
        projectId = projectID,
        callId = callId,
        liftBuildSettings = liftBuildSettings,
        filter = filter,
        filterId = filterID,

      };
    }


    /// <summary>
    /// Create example instance of PassCounts to display in Help documentation.
    /// </summary>
    public new static SummarySpeedRequest HelpSample
    {
      get
      {
        return new SummarySpeedRequest()
               {
                   projectId = 34,
                   callId = Guid.NewGuid(),
                   liftBuildSettings = LiftBuildSettings.HelpSample,
               };
      }
    }

  }
}