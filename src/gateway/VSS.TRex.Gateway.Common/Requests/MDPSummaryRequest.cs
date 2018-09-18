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
    public FilterResult Filter { get; private set; }

    /// <summary>
    /// The target MDP value expressed in 10ths of units
    /// </summary>
    [Range(MIN_MDP, MAX_MDP)]
    [JsonProperty(PropertyName = "mdpTarget", Required = Required.Default)]
    public short MdpTarget { get; private set; }

    /// <summary>
    /// Override the target MDP recorded from the machine with the value of mdpTarget
    /// </summary>
    [JsonProperty(PropertyName = "overrideTargetMDP", Required = Required.Always)]
    [Required]
    public bool OverrideTargetMDP { get; private set; }

    /// <summary>
    /// The maximum percentage the measured MDP may be compared to the mdpTarget from the machine, or the mdpTarget override if overrideTargetMDP is true
    /// </summary>
    [Range(MIN_PERCENT_MDP, MAX_PERCENT_MDP)]
    [JsonProperty(PropertyName = "maxMDPPercent", Required = Required.Default)]
    public double MaxMDPPercent { get; private set; }

    /// <summary>
    /// The minimum percentage the measured MDP may be compared to the mdpTarget from the machine, or the mdpTarget override if overrideTargetMDP is true
    /// </summary>
    [Range(MIN_PERCENT_MDP, MAX_PERCENT_MDP)]
    [JsonProperty(PropertyName = "minMDPPercent", Required = Required.Default)]
    public double MinMDPPercent { get; private set; }

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private MDPSummaryRequest()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public MDPSummaryRequest(
      Guid projectUid,
      FilterResult filter,
      short mdpTarget,
      bool overrideTargetMDP,
      double maxMDPPercent,
      double minMDPPercent
    )
    {
      ProjectUid = projectUid;
      Filter = filter;
      MdpTarget = mdpTarget;
      OverrideTargetMDP = overrideTargetMDP;
      MaxMDPPercent = maxMDPPercent;
      MinMDPPercent = minMDPPercent;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      Filter?.Validate();

      if (OverrideTargetMDP)
      {
        if (!(MdpTarget > 0) || !(MinMDPPercent > 0 && MaxMDPPercent > 0))
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Invalid MDP settings: if overriding Target, Target and MDP Percentage values should be specified."));
        }
      }

      if (MdpTarget > 0)
      {
        if (MinMDPPercent > 0 || MaxMDPPercent > 0)
        {
          if (MinMDPPercent > MaxMDPPercent)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Invalid MDP summary request values: must have minimum % < maximum %"));
          }
        }
      }
    }
  }
}
