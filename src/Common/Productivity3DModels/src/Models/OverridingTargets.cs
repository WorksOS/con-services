using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// Common settings for TRex requests
  /// </summary>
  public class OverridingTargets
  {
    private const ushort MIN_CMV = 0;
    private const ushort MAX_CMV = 1500;
    private const double MIN_PERCENT_CMV = 0.0;
    private const double MAX_PERCENT_CMV = 250.0;

    private const short MIN_MDP = 0;
    private const short MAX_MDP = 10000;
    private const double MIN_PERCENT_MDP = 0.0;
    private const double MAX_PERCENT_MDP = 250.0;

    /// <summary>
    /// The target CMV value expressed in 10ths of units.
    /// </summary>
    [Range(MIN_CMV, MAX_CMV)]
    [JsonProperty(Required = Required.Default)]
    public short CmvTarget { get; private set; }

    /// <summary>
    /// Override the target CMV recorded from the machine with the value of cmvTarget
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public bool OverrideTargetCMV { get; private set; }

    /// <summary>
    /// The minimum percentage the measured CMV may be compared to the cmvTarget from the machine,
    /// or the cmvTarget override if overrideTargetCMV is true
    /// </summary>
    [Range(MIN_PERCENT_CMV, MAX_PERCENT_CMV)]
    [JsonProperty(Required = Required.Default)]
    public double MinCMVPercent { get; private set; }

    /// <summary>
    /// The maximum percentage the measured CMV may be compared to the cmvTarget from the machine,
    /// or the cmvTarget override if overrideTargetCMV is true
    /// </summary>
    [Range(MIN_PERCENT_CMV, MAX_PERCENT_CMV)]
    [JsonProperty(Required = Required.Default)]
    public double MaxCMVPercent { get; private set; }

    /// <summary>
    /// The target MDP value expressed in 10ths of units
    /// </summary>
    [Range(MIN_MDP, MAX_MDP)]
    [JsonProperty(Required = Required.Default)]
    public short MdpTarget { get; private set; }

    /// <summary>
    /// Override the target MDP recorded from the machine with the value of mdpTarget
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public bool OverrideTargetMDP { get; private set; }

    /// <summary>
    /// The maximum percentage the measured MDP may be compared to the mdpTarget from the machine,
    /// or the mdpTarget override if overrideTargetMDP is true
    /// </summary>
    [Range(MIN_PERCENT_MDP, MAX_PERCENT_MDP)]
    [JsonProperty(Required = Required.Default)]
    public double MaxMDPPercent { get; private set; }

    /// <summary>
    /// The minimum percentage the measured MDP may be compared to the mdpTarget from the machine,
    /// or the mdpTarget override if overrideTargetMDP is true
    /// </summary>
    [Range(MIN_PERCENT_MDP, MAX_PERCENT_MDP)]
    [JsonProperty(Required = Required.Default)]
    public double MinMDPPercent { get; private set; }

    /// <summary>
    /// The global override value for target pass count range. Optional.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public TargetPassCountRange OverridingTargetPassCountRange { get; private set; }

    /// <summary>
    /// The various summary and target values to use in preparation of the result
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public TemperatureSettings TemperatureSettings { get; private set; }

    /// <summary>
    /// Sets the machine speed target for Speed Summary requests. During this request Raptor does analysis of all cell passes (filtered out) and searches for the
    /// passes with speed above or below target values. If there is at least one cell pass satisfying the condition - this cell is considered bad.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public MachineSpeedTarget MachineSpeedTarget { get; private set; }


    /// <summary>
    /// Default public constructor.
    /// </summary>
    public OverridingTargets()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public OverridingTargets(
      short cmvTarget=0,
      bool overrideTargetCMV=false,
      double maxCMVPercent=0,
      double minCMVPercent=0,
      short mdpTarget=0,
      bool overrideTargetMDP=false,
      double maxMDPPercent=0,
      double minMDPPercent=0,
      TargetPassCountRange overridingTargetPassCountRange=null,
      TemperatureSettings temperatureSettings=null,
      MachineSpeedTarget machineSpeedTarget=null)
    {
      CmvTarget = cmvTarget;
      OverrideTargetCMV = overrideTargetCMV;
      MaxCMVPercent = maxCMVPercent;
      MinCMVPercent = minCMVPercent;
      MdpTarget = mdpTarget;
      OverrideTargetMDP = overrideTargetMDP;
      MaxMDPPercent = maxMDPPercent;
      MinMDPPercent = minMDPPercent;
      OverridingTargetPassCountRange = overridingTargetPassCountRange;
      TemperatureSettings = temperatureSettings;
      MachineSpeedTarget = machineSpeedTarget;
    }

    /// <summary>
    /// Validates all properties.
    /// </summary>
    public void Validate()
    {
      ValidateTarget(OverrideTargetCMV, CmvTarget, MinCMVPercent, MaxCMVPercent, "CMV");
      ValidateTarget(OverrideTargetMDP, MdpTarget, MinMDPPercent, MaxMDPPercent, "MDP");
      OverridingTargetPassCountRange?.Validate();
      TemperatureSettings?.Validate();
      MachineSpeedTarget?.Validate();
    }

    /// <summary>
    /// Common validation for targets and percentage range
    /// </summary>
    private void ValidateTarget(bool overrideTarget, short target, double minPercent, double maxPercent, string what)
    {
      if (overrideTarget)
      {
        if (!(target > 0) || !(minPercent > 0 && maxPercent > 0))
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, $"Invalid {what} settings: if overriding Target, Target and {what} Percentage values should be specified."));
        }
      }

      if (target > 0)
      {
        if (minPercent > 0 || maxPercent > 0)
        {
          if (minPercent > maxPercent)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, $"Invalid {what} summary request values: must have minimum % < maximum %"));
          }
        }
      }
    }
  }
}
