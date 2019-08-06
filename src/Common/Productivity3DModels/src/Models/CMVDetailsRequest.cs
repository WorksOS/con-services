using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// The request representation used to request CMV details.
  /// </summary>
  public class CMVDetailsRequest : TRexBaseRequest
  {
    /// <summary>
    /// The collection of CMV targets. Values are in ascending order.
    /// There must be 16 values and the first value must be 0.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    [Required]
    public int[] CustomCMVDetailTargets { get; set; }

    /// <summary>
    /// Default private constructor
    /// </summary>
    private CMVDetailsRequest()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public CMVDetailsRequest(
      Guid projectUid,
      FilterResult filter,
      int[] customCMVDetailTargets,
      OverridingTargets overrides,
      LiftSettings liftSettings
    )
    {
      ProjectUid = projectUid;
      Filter = filter;
      CustomCMVDetailTargets = customCMVDetailTargets;
      Overrides = overrides;
      LiftSettings = liftSettings;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      // Validate custom CMV Detail targets...
      if (CustomCMVDetailTargets == null || CustomCMVDetailTargets.Length == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "CMV Detail targets required"));
      }
      if (CustomCMVDetailTargets[0] != MIN_CMV)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"CMV Detail targets must start at {MIN_CMV}"));
      }
      for (int i = 1; i < CustomCMVDetailTargets.Length; i++)
      {
        if (CustomCMVDetailTargets[i] <= CustomCMVDetailTargets[i - 1])
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "CMV Detail targets must be ordered from lowest to the highest"));
        }
      }
      if (CustomCMVDetailTargets[CustomCMVDetailTargets.Length - 1] < MIN_CMV || CustomCMVDetailTargets[CustomCMVDetailTargets.Length - 1] > MAX_CMV)
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
