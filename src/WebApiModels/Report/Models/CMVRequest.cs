using System;
using System.Collections.Generic;
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
  /// The request representation used to request both detailed and summary CMV requests.
  /// </summary>
  public class CMVRequest : ProjectID, IValidatable
  {
    /// <summary>
    /// An identifying string from the caller
    /// </summary>
    [JsonProperty(PropertyName = "callId", Required = Required.Default)]
    public Guid? CallId { get; private set; }

    /// <summary>
    /// The various summary and target values to use in preparation of the result
    /// </summary>
    [JsonProperty(PropertyName = "cmvSettings", Required = Required.Always)]
    [Required]
    public CMVSettings CmvSettings { get; private set; }

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
    public long FilterID { get; private set; }

    /// <summary>
    /// An override start date that applies to the operation in conjunction with any date range specified in a filter.
    /// Value may be null
    /// </summary>
    [JsonProperty(PropertyName = "overrideStartUTC", Required = Required.Default)]
    public DateTime? OverrideStartUTC { get; private set; }

    /// <summary>
    /// An override end date that applies to the operation in conjunction with any date range specified in a filter.
    /// Value may be null
    /// </summary>
    [JsonProperty(PropertyName = "overrideEndUTC", Required = Required.Default)]
    public DateTime? OverrideEndUTC { get; private set; }

    /// <summary>
    /// An override set of asset IDs that applies to the operation in conjunction with any asset IDs specified in a filter.
    /// Value may be null
    /// </summary>
    [JsonProperty(PropertyName = "overrideAssetIds", Required = Required.Default)]
    public List<long> OverrideAssetIds { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonProperty(PropertyName = "isCustomCMVTargets", Required = Required.Default)]
    public bool IsCustomCMVTargets { get; private set; }

      /// <summary>
    /// Default private constructor
    /// </summary>
    private CMVRequest()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="projectID"></param>
    /// <param name="projectUid"></param>
    /// <param name="callId"></param>
    /// <param name="cmvSettings"></param>
    /// <param name="liftBuildSettings"></param>
    /// <param name="filter"></param>
    /// <param name="filterID"></param>
    /// <param name="overrideStartUTC"></param>
    /// <param name="overrideEndUTC"></param>
    /// <param name="overrideAssetIds"></param>
    /// <param name="isCustomCMVTargets"></param>
    public CMVRequest(
      long projectID,
      Guid? projectUid,
      Guid? callId,
      CMVSettings cmvSettings,
      LiftBuildSettings liftBuildSettings,
      FilterResult filter,
      long filterID,
      DateTime? overrideStartUTC,
      DateTime? overrideEndUTC,
      List<long> overrideAssetIds,
      bool isCustomCMVTargets = false
    )
    {
      ProjectId = projectID;
      ProjectUid = projectUid;
      CallId = callId;
      CmvSettings = cmvSettings;
      LiftBuildSettings = liftBuildSettings;
      Filter = filter;
      FilterID = filterID;
      OverrideStartUTC = overrideStartUTC;
      OverrideEndUTC = overrideEndUTC;
      OverrideAssetIds = overrideAssetIds;
      IsCustomCMVTargets = isCustomCMVTargets;
    }
    
    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();
      CmvSettings.Validate();
      LiftBuildSettings?.Validate();

      if (Filter != null)
        Filter.Validate();

      if (OverrideStartUTC.HasValue || OverrideEndUTC.HasValue)
      {
        if (OverrideStartUTC.HasValue && OverrideEndUTC.HasValue)
        {
          if (OverrideStartUTC.Value > OverrideEndUTC.Value)
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
}
