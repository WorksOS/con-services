using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.WebApi.Models.Report.Models
{
  /// <summary>
  /// The representation of a pass counts request
  /// </summary>
  public class PassCounts : ProjectID
  {
    /// <summary>
    /// An identifier from the caller. 
    /// </summary>
    [JsonProperty(PropertyName = "callId", Required = Required.Default)]
    public Guid? callId { get; protected set; }

    /// <summary>
    /// Setting and configuration values related to processing pass count related queries
    /// </summary>
    [JsonProperty(PropertyName = "passCountSettings", Required = Required.Default)]
    public PassCountSettings passCountSettings { get; protected set; }

    /// <summary>
    /// A collection of parameters and configuration information relating to analysis and determination of material layers.
    /// </summary>
    [JsonProperty(PropertyName = "liftBuildSettings", Required = Required.Default)]
    public LiftBuildSettings liftBuildSettings { get; protected set; }

    /// <summary>
    /// The filter instance to use in the request
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult filter { get; protected set; }

    /// <summary>
    /// The filter ID to used in the request.
    /// May be null.
    /// </summary>
    [JsonProperty(PropertyName = "filterID", Required = Required.Default)]
    public long filterID { get; protected set; }

    /// <summary>
    /// An override start date that applies to the operation in conjunction with any date range specified in a filter.
    /// Value may be null
    /// </summary>
    [JsonProperty(PropertyName = "overrideStartUTC", Required = Required.Default)]
    public DateTime? overrideStartUTC { get; protected set; }

    /// <summary>
    /// An override end date that applies to the operation in conjunction with any date range specified in a filter.
    /// Value may be null
    /// </summary>
    [JsonProperty(PropertyName = "overrideEndUTC", Required = Required.Default)]
    public DateTime? overrideEndUTC { get; protected set; }

    /// <summary>
    /// An override set of asset IDs that applies to the operation in conjunction with any asset IDs specified in a filter.
    /// </summary>
    [JsonProperty(PropertyName = "overrideAssetIds", Required = Required.Default)]
    public List<long> overrideAssetIds { get; protected set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    protected PassCounts()
    {}

    /// <summary>
    /// Create instance of CreatePassCountsRequest
    /// </summary>
    public static PassCounts CreatePassCountsRequest(
        long projectId,
        Guid? callId,
        PassCountSettings passCountSettings,
        LiftBuildSettings liftBuildSettings,
        FilterResult filter,
        long filterID,
        DateTime? overrideStartUTC,
        DateTime? overrideEndUTC,
        List<long> overrideAssetIds
        )
    {
      return new PassCounts
             {
                 projectId = projectId,
                 passCountSettings = passCountSettings,
                 liftBuildSettings = liftBuildSettings,
                 filter = filter,
                 filterID = filterID,
                 overrideStartUTC = overrideStartUTC,
                 overrideEndUTC = overrideEndUTC,
                 overrideAssetIds = overrideAssetIds
             };
    }
    
    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();
      //pass count settings only required for detailed
      passCountSettings?.Validate();
      liftBuildSettings?.Validate();

      if (filter != null)
        filter.Validate();

      if (overrideStartUTC.HasValue || overrideEndUTC.HasValue)
      {
        if (overrideStartUTC.HasValue && overrideEndUTC.HasValue)
        {
          if (overrideStartUTC.Value > overrideEndUTC.Value)
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