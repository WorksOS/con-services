using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// Represents CMV change details request.
  /// </summary>
  public class CMVChangeDetailsRequest : TRexBaseRequest
  {
    /// <summary>
    /// Sets the CMV change details values to compare against.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    [Required]
    public double[] CMVChangeDetailsValues { get; set; }

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private CMVChangeDetailsRequest()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public CMVChangeDetailsRequest(
      Guid projectUid,
      FilterResult filter,
      double[] cmvChangeDetailsValues,
      OverridingTargets overrides,
      LiftSettings liftSettings
    )
    {
      ProjectUid = projectUid;
      Filter = filter;
      CMVChangeDetailsValues = cmvChangeDetailsValues;
      Overrides = overrides;
      LiftSettings = liftSettings;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      for (int i = 1; i < CMVChangeDetailsValues.Length; i++)
        if (CMVChangeDetailsValues[i] <= CMVChangeDetailsValues[i - 1])
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "CMVChangeDetailsValues should be in ascending order."));
    }
  }
}
