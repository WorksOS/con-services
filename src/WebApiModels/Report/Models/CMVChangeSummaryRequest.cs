using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApiModels.Report.Models
{
  /// <summary>
  /// Represents speed summary request.
  /// </summary>
  public class CMVChangeSummaryRequest : ProjectID, IValidatable
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
    public FilterResult filter { get; protected set; }

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


    /// <summary>
    /// Sets the CMV change summary values to compare against.
    /// </summary>
    [JsonProperty(PropertyName = "CMVChangeSummaryValues", Required = Required.Always)]
    [Required]
    public double[] CMVChangeSummaryValues { get; private set; }

    public override void Validate()
    {
      base.Validate();
      if (this.liftBuildSettings != null)
        this.liftBuildSettings.Validate();

      if (this.filter !=null)
        this.filter.Validate();

      for (int i=1;i<CMVChangeSummaryValues.Length;i++ )
        if (CMVChangeSummaryValues[i] <= CMVChangeSummaryValues[i - 1])
          throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "CMVChangeSummaryValues should be in ascending order."));

    }

    /// <summary>
    /// Private constructor
    /// </summary>
    private CMVChangeSummaryRequest()
    {
    }

    /// <summary>
    /// Create instance of CMVRequest
    /// </summary>
    public static CMVChangeSummaryRequest CreateCMVChangeSummaryRequest(
      long projectID,
      Guid? callId,
      LiftBuildSettings liftBuildSettings,
      FilterResult filter,
      int filterID,
      double[] cmvChangeSummaryValues
        )
    {
      return new CMVChangeSummaryRequest
      {
        ProjectId = projectID,
        callId = callId,
        liftBuildSettings = liftBuildSettings,
        filter = filter,
        filterId = filterID,
        CMVChangeSummaryValues = cmvChangeSummaryValues
      };
    }
  }
}
