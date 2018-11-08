using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.Report.Models
{
  /// <summary>
  /// Represents CMV change summary request.
  /// </summary>
  public class CMVChangeSummaryRequest : ProjectID, IValidatable
  {

    /// <summary>
    /// An identifying string from the caller
    /// </summary>
    [JsonProperty(PropertyName = "callId", Required = Required.Default)]
    public Guid? CallId { get; protected set; }

    /// <summary>
    /// The filter to be used 
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult Filter { get; protected set; }

    /// <summary>
    /// Gets or sets the filter identifier.
    /// </summary>
    /// <value>
    /// The filter identifier.
    /// </value>
    [JsonProperty(PropertyName = "filterId", Required = Required.Default)]
    public int FilterId { get; protected set; }

    /// <summary>
    /// The set of parameters and configuration information relevant to analysis of compaction material layers information for related profile queries.
    /// </summary>
    [JsonProperty(PropertyName = "liftBuildSettings", Required = Required.Default)]
    public LiftBuildSettings LiftBuildSettings { get; protected set; }


    /// <summary>
    /// Sets the CMV change summary values to compare against.
    /// </summary>
    [JsonProperty(PropertyName = "CMVChangeSummaryValues", Required = Required.Always)]
    [Required]
    public double[] CMVChangeSummaryValues { get; private set; }

    public override void Validate()
    {
      base.Validate();
      if (this.LiftBuildSettings != null)
        this.LiftBuildSettings.Validate();

      if (this.Filter !=null)
        this.Filter.Validate();

      for (int i=1;i<CMVChangeSummaryValues.Length;i++ )
        if (CMVChangeSummaryValues[i] <= CMVChangeSummaryValues[i - 1])
          throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "CMVChangeSummaryValues should be in ascending order."));

    }

    /// <summary>
    /// Default private constructor
    /// </summary>
    private CMVChangeSummaryRequest()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="projectID"></param>
    /// <param name="projectUid"></param>
    /// <param name="callId"></param>
    /// <param name="liftBuildSettings"></param>
    /// <param name="filter"></param>
    /// <param name="filterID"></param>
    /// <param name="cmvChangeSummaryValues"></param>
    public CMVChangeSummaryRequest(
      long projectID,
      Guid? projectUid,
      Guid? callId,
      LiftBuildSettings liftBuildSettings,
      FilterResult filter,
      int filterID,
      double[] cmvChangeSummaryValues
     )
    {
      ProjectId = projectID;
      ProjectUid = projectUid;
      CallId = callId;
      LiftBuildSettings = liftBuildSettings;
      Filter = filter;
      FilterId = filterID;
      CMVChangeSummaryValues = cmvChangeSummaryValues;
    }
  }
}
