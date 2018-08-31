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
    /// <summary>
    /// The target MDP value expressed in 10ths of units
    /// </summary>
    [Range(MIN_MDP, MAX_MDP)]
    [JsonProperty(PropertyName = "mdpTarget", Required = Required.Default)]
    public short mdpTarget { get; private set; }

    /// <summary>
    /// The maximum MDP value to be considered 'compacted' expressed in 10ths of units
    /// </summary>
    [Range(MIN_MDP, MAX_MDP)]
    [JsonProperty(PropertyName = "maxMDP", Required = Required.Default)]
    public short maxMDP { get; private set; }

    /// <summary>
    /// The maximum percentage the measured MDP may be compared to the mdpTarget from the machine, or the mdpTarget override if overrideTargetMDP is true
    /// </summary>
    [Range(MIN_PERCENT_MDP, MAX_PERCENT_MDP)]
    [JsonProperty(PropertyName = "maxMDPPercent", Required = Required.Default)]
    public double maxMDPPercent { get; private set; }

    /// <summary>
    /// The minimum MDP value to be considered 'compacted' expressed in 10ths of units
    /// </summary>
    [Range(MIN_MDP, MAX_MDP)]
    [JsonProperty(PropertyName = "minMDP", Required = Required.Default)]
    public short minMDP { get; private set; }

    /// <summary>
    /// The minimum percentage the measured MDP may be compared to the mdpTarget from the machine, or the mdpTarget override if overrideTargetMDP is true
    /// </summary>
    [Range(MIN_PERCENT_MDP, MAX_PERCENT_MDP)]
    [JsonProperty(PropertyName = "minMDPPercent", Required = Required.Default)]
    public double minMDPPercent { get; private set; }

    /// <summary>
    /// Override the target MDP recorded from the machine with the value of mdpTarget
    /// </summary>
    [JsonProperty(PropertyName = "overrideTargetMDP", Required = Required.Always)]
    [Required]
    public bool overrideTargetMDP { get; private set; }

    /// <summary>
    /// Defailt private constructor
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
      this.mdpTarget = mdpTarget;
      this.maxMDP = maxMDP;
      this.maxMDPPercent = maxMDPPercent;
      this.minMDP = minMDP;
      this.minMDPPercent = minMDPPercent;
      this.overrideTargetMDP = overrideTargetMDP;
    }


    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (overrideTargetMDP)
        if (!(mdpTarget > 0) || !((minMDP > 0 && maxMDP > 0) || (minMDPPercent > 0) && (maxMDPPercent > 0)))
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
                new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Invalid MDP settings: if overriding Target, Target and (MDP Percentage or MDP values) shall be specified."));
        }

      if (mdpTarget > 0)
      {
        if (minMDPPercent > 0 || maxMDPPercent > 0)
          ValidateRange(minMDPPercent, maxMDPPercent);
        if (minMDP > 0 || maxMDP > 0)
        {
          ValidateRange(minMDP, maxMDP);
          ValidateRange(minMDP, mdpTarget);
          ValidateRange(mdpTarget, maxMDP);
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

    private const short MIN_MDP = 0;
    private const short MAX_MDP = 10000;
    private const double MIN_PERCENT_MDP = 0.0;
    private const double MAX_PERCENT_MDP = 250.0;

  }
}
