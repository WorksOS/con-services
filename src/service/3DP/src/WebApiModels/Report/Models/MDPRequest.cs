using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.Report.Models
{
  /// <summary>
  /// The request representation used to request both detailed and summary MDP requests.
  /// </summary>
  public class MDPRequest : ProjectID, IValidatable
  {
    /// <summary>
    /// An identifying string from the caller
    /// </summary>
    [JsonProperty(PropertyName = "callId", Required = Required.Default)]
    public Guid? CallId { get; private set; }

    /// <summary>
    /// The various summary and target values to use in preparation of the result
    /// </summary>
    [JsonProperty(PropertyName = "mdpSettings", Required = Required.Always)]
    [Required]
    public MDPSettings MdpSettings { get; private set; }

    /// <summary>
    /// The lift build settings to use in the request.
    /// </summary>
    [JsonProperty(PropertyName = "liftBuildSettings", Required = Required.Default)]
    public LiftBuildSettings LiftBuildSettings { get; private set; }

    /// <summary>
    /// The filter instance to use in the request
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult Filter { get; private set; }

    /// <summary>
    /// The filter ID to used in the request.
    /// May be null.
    /// </summary>
    [JsonProperty(PropertyName = "filterID", Required = Required.Default)]
    public long FilterId { get; private set; }

    /// <summary>
    /// An override start date that applies to the operation in conjunction with any date range specified in a filter.
    /// Value may be null
    /// </summary>
    [JsonProperty(PropertyName = "overrideStartUTC", Required = Required.Default)]
    public DateTime? OverrideStartUtc { get; private set; }

    /// <summary>
    /// An override end date that applies to the operation in conjunction with any date range specified in a filter.
    /// Value may be null
    /// </summary>
    [JsonProperty(PropertyName = "overrideEndUTC", Required = Required.Default)]
    public DateTime? OverrideEndUtc { get; private set; }

    /// <summary>
    /// An override set of asset IDs that applies to the operation in conjunction with any asset IDs specified in a filter.
    /// Value may be null
    /// </summary>
    [JsonProperty(PropertyName = "overrideAssetIds", Required = Required.Default)]
    public List<long> OverrideAssetIds { get; private set; }

    /// <summary>
    /// Default private constructor
    /// </summary>
    protected MDPRequest()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="projectID"></param>
    /// <param name="projectUID"></param>
    /// <param name="callId"></param>
    /// <param name="mdpSettings"></param>
    /// <param name="liftBuildSettings"></param>
    /// <param name="filter"></param>
    /// <param name="filterId"></param>
    /// <param name="overrideStartUtc"></param>
    /// <param name="overrideEndUtc"></param>
    /// <param name="overrideAssetIds"></param>
    /// <returns></returns>
    public MDPRequest(
      long projectID,
      Guid? projectUID,
      Guid? callId,
      MDPSettings mdpSettings,
      LiftBuildSettings liftBuildSettings,
      FilterResult filter,
      long filterId,
      DateTime? overrideStartUtc,
      DateTime? overrideEndUtc,
      List<long> overrideAssetIds)
    {
      ProjectId = projectID;
      ProjectUid = projectUID;
      CallId = callId;
      MdpSettings = mdpSettings;
      LiftBuildSettings = liftBuildSettings;
      Filter = filter;
      FilterId = filterId;
      OverrideStartUtc = overrideStartUtc;
      OverrideEndUtc = overrideEndUtc;
      OverrideAssetIds = overrideAssetIds;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();
      MdpSettings.Validate();
      LiftBuildSettings?.Validate();

      if (Filter != null)
        Filter.Validate();

      if (!OverrideStartUtc.HasValue && !OverrideEndUtc.HasValue)
      {
        return;
      }

      if (OverrideStartUtc.HasValue && OverrideEndUtc.HasValue)
      {
        if (OverrideStartUtc.Value > OverrideEndUtc.Value)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Override startUTC must be earlier than override endUTC"));
        }
      }
      else
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "If using an override date range both dates must be provided"));
      }
    }
  }
}
