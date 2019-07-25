using System;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Validation;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// Base class for TRex summary requests
  /// </summary>
  public abstract class TRexSummaryRequest 
  {
    /// <summary>
    /// A project unique identifier.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    [ValidProjectUID]
    public Guid ProjectUid { get; set; }

    /// <summary>
    /// The filter instance to use in the request.
    /// Value may be null.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public FilterResult Filter { get; set; }

    /// <summary>
    /// Overriding targets for the type of summary request
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public OverridingTargets Overrides { get; set; }

    /// <summary>
    /// Settings for lift analysis
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public LiftSettings LiftSettings { get; set; }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      new DataAnnotationsValidator().TryValidate(this, out var results);

      if (results.Any())
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, results.FirstOrDefault()?.ErrorMessage));
      }

      Filter?.Validate();
      Overrides?.Validate();
      LiftSettings?.Validate();
    }

  }
}
