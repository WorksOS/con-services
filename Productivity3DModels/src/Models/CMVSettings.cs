using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// The parameters for CMV detailed and summary computations
  /// </summary>
  public class CMVSettings //: IValidatable
  {
    /// <summary>
    /// The target CMV value expressed in 10ths of units
    /// </summary>
    [Range(MIN_CMV, MAX_CMV)]
    [JsonProperty(PropertyName = "cmvTarget", Required = Required.Default)]
    public short cmvTarget { get; protected set; }

    /// <summary>
    /// The maximum CMV value to be considered 'compacted' expressed in 10ths of units
    /// </summary>
    [Range(MIN_CMV, MAX_CMV)]
    [JsonProperty(PropertyName = "maxCMV", Required = Required.Default)]
    public short maxCMV { get; protected set; }

    /// <summary>
    /// The maximum percentage the measured CMV may be compared to the cmvTarget from the machine, or the cmvTarget override if overrideTargetCMV is true
    /// </summary>
    [Range(MIN_PERCENT_CMV, MAX_PERCENT_CMV)]
    [JsonProperty(PropertyName = "maxCMVPercent", Required = Required.Default)]
    public double maxCMVPercent { get; protected set; }

    /// <summary>
    /// The minimum CMV value to be considered 'compacted' expressed in 10ths of units
    /// </summary>
    [Range(MIN_CMV, MAX_CMV)]
    [JsonProperty(PropertyName = "minCMV", Required = Required.Default)]
    public short minCMV { get; protected set; }

    /// <summary>
    /// The minimum percentage the measured CMV may be compared to the cmvTarget from the machine, or the cmvTarget override if overrideTargetCMV is true
    /// </summary>
    [Range(MIN_PERCENT_CMV, MAX_PERCENT_CMV)]
    [JsonProperty(PropertyName = "minCMVPercent", Required = Required.Default)]
    public double minCMVPercent { get; protected set; }

    /// <summary>
    /// Override the target CMV recorded from the machine with the value of cmvTarget
    /// </summary>
    [JsonProperty(PropertyName = "overrideTargetCMV", Required = Required.Default)]
    [Required]
    public bool overrideTargetCMV { get; protected set; }

    protected const ushort MIN_CMV = 0;
    protected const ushort MAX_CMV = 1500;
    private const double MIN_PERCENT_CMV = 0.0;
    private const double MAX_PERCENT_CMV = 250.0;

    /// <summary>
    /// Private constructor
    /// </summary>
    protected CMVSettings()
    {
    }

    /// <summary>
    /// Create instance of CMVSettings
    /// </summary>
    public static CMVSettings CreateCMVSettings(
        short cmvTarget,
        short maxCMV,
        double maxCMVPercent,
        short minCMV,
        double minCMVPercent,
        bool overrideTargetCMV
        )
    {
      return new CMVSettings
      {
        cmvTarget = cmvTarget,
        maxCMV = maxCMV,
        maxCMVPercent = maxCMVPercent,
        minCMV = minCMV,
        minCMVPercent = minCMVPercent,
        overrideTargetCMV = overrideTargetCMV
      };
    }
    
    /// <summary>
    /// Validates all properties
    /// </summary>
    public virtual void Validate()
    {
      if (overrideTargetCMV)
      {
        if (!(cmvTarget > 0) || !(minCMV > 0 && maxCMV > 0 || minCMVPercent > 0 && maxCMVPercent > 0))
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Invalid CMV settings: if overriding Target, Target and (CMV Percentage or CMV values) shall be specified."));
        }
      }

      if (cmvTarget > 0)
      {
        if (minCMVPercent > 0 || maxCMVPercent > 0)
        {
          ValidateRange(minCMVPercent, maxCMVPercent);
        }

        if (minCMV > 0 || maxCMV > 0)
        {
          ValidateRange(minCMV, maxCMV);
          ValidateRange(minCMV, cmvTarget);
          ValidateRange(cmvTarget, maxCMV);
        }
      }
    }

    private static void ValidateRange(double lower, double upper)
    {
      if (lower > upper)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Invalid CMV settings values: must have minimum < target < maximum and minimum % < maximum %"));
      }
    }
  }
}