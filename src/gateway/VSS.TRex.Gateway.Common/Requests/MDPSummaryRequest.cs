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
  /// The request representation used to request MDP summary.
  /// </summary>
  public class MDPSummaryRequest : ProjectID
  {
    private const short MIN_MDP = 0;
    private const short MAX_MDP = 10000;
    private const double MIN_PERCENT_MDP = 0.0;
    private const double MAX_PERCENT_MDP = 250.0;

    /// <summary>
    /// The filter instance to use in the request.
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult filter { get; private set; }

    /// <summary>
    /// The target MDP value expressed in 10ths of units
    /// </summary>
    [Range(MIN_MDP, MAX_MDP)]
    [JsonProperty(PropertyName = "mdpTarget", Required = Required.Default)]
    public short mdpTarget { get; private set; }

    /// <summary>
    /// Override the target MDP recorded from the machine with the value of mdpTarget
    /// </summary>
    [JsonProperty(PropertyName = "overrideTargetMDP", Required = Required.Always)]
    [Required]
    public bool overrideTargetMDP { get; private set; }

    /// <summary>
    /// The maximum percentage the measured MDP may be compared to the mdpTarget from the machine, or the mdpTarget override if overrideTargetMDP is true
    /// </summary>
    [Range(MIN_PERCENT_MDP, MAX_PERCENT_MDP)]
    [JsonProperty(PropertyName = "maxMDPPercent", Required = Required.Default)]
    public double maxMDPPercent { get; private set; }

    /// <summary>
    /// The minimum percentage the measured MDP may be compared to the mdpTarget from the machine, or the mdpTarget override if overrideTargetMDP is true
    /// </summary>
    [Range(MIN_PERCENT_MDP, MAX_PERCENT_MDP)]
    [JsonProperty(PropertyName = "minMDPPercent", Required = Required.Default)]
    public double minMDPPercent { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private MDPSummaryRequest()
    {
    }

    /// <summary>
    /// Create an instance of the MDPSummaryRequest class.
    /// </summary>
    public static MDPSummaryRequest CreateMDPSummaryRequest(
      Guid projectUid,
      FilterResult filter,
      short mdpTarget,
      bool overrideTargetMDP,
      double maxMDPPercent,
      double minMDPPercent
    )
    {
      return new MDPSummaryRequest
      {
        ProjectUid = projectUid,
        filter = filter,
        mdpTarget = mdpTarget,
        overrideTargetMDP = overrideTargetMDP,
        maxMDPPercent = maxMDPPercent,
        minMDPPercent = minMDPPercent
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      filter?.Validate();

      if (overrideTargetMDP)
      {
        if (!(mdpTarget > 0) || !(minMDPPercent > 0 && maxMDPPercent > 0))
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Invalid MDP settings: if overriding Target, Target and MDP Percentage values should be specified."));
        }
      }

      if (mdpTarget > 0)
      {
        if (minMDPPercent > 0 || maxMDPPercent > 0)
        {
          if (minMDPPercent > maxMDPPercent)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Invalid MDP summary request values: must have minimum % < maximum %"));
          }
        }
      }
    }
  }
}
