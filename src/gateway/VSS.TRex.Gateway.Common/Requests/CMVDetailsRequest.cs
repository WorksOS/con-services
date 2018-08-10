using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.TRex.Gateway.Common.Requests
{
  /// <summary>
  /// The request representation used to request CMV details.
  /// </summary>
  public class CMVDetailsRequest : ProjectID
  {
    /// <summary>
    /// The filter instance to use in the request
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult filter { get; private set; }

    /// <summary>
    /// The collection of CMV targets. Values are in ascending order.
    /// There must be 16 values and the first value must be 0.
    /// </summary>
    [JsonProperty(PropertyName = "customCMVDetailTargets", Required = Required.Always)]
    public int[] customCMVDetailTargets { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private CMVDetailsRequest()
    {
    }

    /// <summary>
    /// Create an instance of the CMVDetailsRequest class.
    /// </summary>
    public static CMVDetailsRequest CreateCMVDetailsRequest(
      Guid projectUid,
      FilterResult filter,
      int[] customCMVDetailTargets
    )
    {
      return new CMVDetailsRequest
      {
        ProjectUid = projectUid,
        filter = filter,
        customCMVDetailTargets = customCMVDetailTargets
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      filter?.Validate();

      // Validate custom CMV Detail targets...
      if (customCMVDetailTargets == null || customCMVDetailTargets.Length == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "CMV Detail targets required"));
      }
      if (customCMVDetailTargets[0] != MIN_CMV)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"CMV Detail targets must start at {MIN_CMV}"));
      }
      for (int i = 1; i < customCMVDetailTargets.Length; i++)
      {
        if (customCMVDetailTargets[i] <= customCMVDetailTargets[i - 1])
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "CMV Detail targets must be ordered from lowest to the highest"));
        }
      }
      if (customCMVDetailTargets[customCMVDetailTargets.Length - 1] < MIN_CMV || customCMVDetailTargets[customCMVDetailTargets.Length - 1] > MAX_CMV)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"CMV Detail targets must be between {MIN_CMV + 1} and {MAX_CMV}"));
      }
    }

    private const ushort MIN_CMV = 0;
    private const ushort MAX_CMV = 1500;
  }
}
