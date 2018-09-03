using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// The parameters for MDP detailed and summary computations
  /// </summary>
  public class MDPSettings //: IValidatable
  {
    private const short MIN_MDP = 0;
    private const short MAX_MDP = 10000;
    private const double MIN_PERCENT_MDP = 0.0;
    private const double MAX_PERCENT_MDP = 250.0;

    /// <summary>
    /// The target MDP value expressed in 10ths of units
    /// </summary>
    [Range(MIN_MDP, MAX_MDP)]
    [JsonProperty(PropertyName = "mdpTarget", Required = Required.Default)]
    public short MdpTarget { get; private set; }

    /// <summary>
    /// The maximum MDP value to be considered 'compacted' expressed in 10ths of units
    /// </summary>
    [Range(MIN_MDP, MAX_MDP)]
    [JsonProperty(PropertyName = "maxMDP", Required = Required.Default)]
    public short MaxMDP { get; private set; }

    /// <summary>
    /// The maximum percentage the measured MDP may be compared to the mdpTarget from the machine, or the mdpTarget override if overrideTargetMDP is true
    /// </summary>
    [Range(MIN_PERCENT_MDP, MAX_PERCENT_MDP)]
    [JsonProperty(PropertyName = "maxMDPPercent", Required = Required.Default)]
    public double MaxMDPPercent { get; private set; }

    /// <summary>
    /// The minimum MDP value to be considered 'compacted' expressed in 10ths of units
    /// </summary>
    [Range(MIN_MDP, MAX_MDP)]
    [JsonProperty(PropertyName = "minMDP", Required = Required.Default)]
    public short MinMDP { get; private set; }

    /// <summary>
    /// The minimum percentage the measured MDP may be compared to the mdpTarget from the machine, or the mdpTarget override if overrideTargetMDP is true
    /// </summary>
    [Range(MIN_PERCENT_MDP, MAX_PERCENT_MDP)]
    [JsonProperty(PropertyName = "minMDPPercent", Required = Required.Default)]
    public double MinMDPPercent { get; private set; }

    /// <summary>
    /// Override the target MDP recorded from the machine with the value of mdpTarget
    /// </summary>
    [JsonProperty(PropertyName = "overrideTargetMDP", Required = Required.Always)]
    [Required]
    public bool OverrideTargetMDP { get; private set; }

    /// <summary>
    /// Default private constructor
    /// </summary>
    private MDPSettings()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="mdpTarget"></param>
    /// <param name="maxMDP"></param>
    /// <param name="maxMDPPercent"></param>
    /// <param name="minMDP"></param>
    /// <param name="minMDPPercent"></param>
    /// <param name="overrideTargetMDP"></param>
    public MDPSettings
    (
      short mdpTarget,
      short maxMDP,
      double maxMDPPercent,
      short minMDP,
      double minMDPPercent,
      bool overrideTargetMDP
    )
    {
      MdpTarget = mdpTarget;
      MaxMDP = maxMDP;
      MaxMDPPercent = maxMDPPercent;
      MinMDP = minMDP;
      MinMDPPercent = minMDPPercent;
      OverrideTargetMDP = overrideTargetMDP;
    }


    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (OverrideTargetMDP)
        if (!(MdpTarget > 0) || !((MinMDP > 0 && MaxMDP > 0) || (MinMDPPercent > 0) && (MaxMDPPercent > 0)))
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
                new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Invalid MDP settings: if overriding Target, Target and (MDP Percentage or MDP values) shall be specified."));
        }

      if (MdpTarget > 0)
      {
        if (MinMDPPercent > 0 || MaxMDPPercent > 0)
          ValidateRange(MinMDPPercent, MaxMDPPercent);
        if (MinMDP > 0 || MaxMDP > 0)
        {
          ValidateRange(MinMDP, MaxMDP);
          ValidateRange(MinMDP, MdpTarget);
          ValidateRange(MdpTarget, MaxMDP);
        }
      }
    }

    private void ValidateRange(double lower, double upper)
    {
      if (lower > upper)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Invalid MDP settings values: must have minimum < target < maximum and minimum % < maximum %"));
      }
    }
  }
}
