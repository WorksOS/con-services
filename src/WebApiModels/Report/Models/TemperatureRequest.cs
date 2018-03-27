using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.WebApiModels.Report.Models
{
  /// <summary>
  /// The request representation used to request summary Temperature requests.
  /// </summary>
  public class TemperatureRequest : ProjectID, IValidatable
  {
    /// <summary>
    /// An identifying string from the caller
    /// </summary>
    [JsonProperty(PropertyName = "callId", Required = Required.Default)]
    public Guid? callId { get; private set; }

    /// <summary>
    /// The various summary and target values to use in preparation of the result
    /// </summary>
    [JsonProperty(PropertyName = "temperatureSettings", Required = Required.Always)]
    [Required]
    public TemperatureSettings temperatureSettings { get; private set; }

    /// <summary>
    /// The lift build settings to use in the request.
    /// </summary>
    [JsonProperty(PropertyName = "liftBuildSettings", Required = Required.Default)]
    public LiftBuildSettings liftBuildSettings { get; private set; }

    /// <summary>
    /// The filter instance to use in the request
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult filter { get; private set; }

    /// <summary>
    /// The filter ID to used in the request.
    /// May be null.
    /// </summary>
    [JsonProperty(PropertyName = "filterID", Required = Required.Default)]
    public long filterID { get; private set; }

    /// <summary>
    /// An override start date that applies to the operation in conjunction with any date range specified in a filter.
    /// Value may be null
    /// </summary>
    [JsonProperty(PropertyName = "overrideStartUTC", Required = Required.Default)]
    public DateTime? overrideStartUTC { get; private set; }

    /// <summary>
    /// An override end date that applies to the operation in conjunction with any date range specified in a filter.
    /// Value may be null
    /// </summary>
    [JsonProperty(PropertyName = "overrideEndUTC", Required = Required.Default)]
    public DateTime? overrideEndUTC { get; private set; }

    /// <summary>
    /// An override set of asset IDs that applies to the operation in conjunction with any asset IDs specified in a filter.
    /// Value may be null
    /// </summary>
    [JsonProperty(PropertyName = "overrideAssetIds", Required = Required.Default)]
    public List<long> overrideAssetIds { get; private set; }

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private TemperatureRequest()
    { }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static TemperatureRequest CreateTemperatureRequest(
      long projectID,
      Guid? callId,
      TemperatureSettings temperatureSettings,
      LiftBuildSettings liftBuildSettings,
      FilterResult filter,
      long filterID,
      DateTime? overrideStartUTC,
      DateTime? overrideEndUTC,
      List<long> overrideAssetIds
        )
    {
      return new TemperatureRequest
      {
        projectId = projectID,
        callId = callId,
        temperatureSettings = temperatureSettings,
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
      temperatureSettings.Validate();
      if (liftBuildSettings != null)
        liftBuildSettings.Validate();
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
