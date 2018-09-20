using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.TRex.Gateway.Common.Requests
{
  /// <summary>
  /// Represents CMV change details request.
  /// </summary>
  public class CMVChangeDetailsRequest : ProjectID
  {
    /// <summary>
    /// The filter instance to use in the request
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult Filter { get; private set; }

    /// <summary>
    /// Sets the CMV change details values to compare against.
    /// </summary>
    [JsonProperty(PropertyName = "CMVChangeSummaryValues", Required = Required.Always)]
    [Required]
    public double[] CMVChangeDetailsValues { get; private set; }

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
      double[] cmvChangeDetailsValues
    )
    {
      ProjectUid = projectUid;
      Filter = filter;
      CMVChangeDetailsValues = cmvChangeDetailsValues;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      Filter?.Validate();

      for (int i = 1; i < CMVChangeDetailsValues.Length; i++)
        if (CMVChangeDetailsValues[i] <= CMVChangeDetailsValues[i - 1])
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "CMVChangeDetailsValues should be in ascending order."));
    }
  }
}
