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
    protected const ushort MIN_CMV = 0;
    protected const ushort MAX_CMV = 1500;
    private const double MIN_PERCENT_CMV = 0.0;
    private const double MAX_PERCENT_CMV = 250.0;

    /// <summary>
    /// The target CMV value expressed in 10ths of units
    /// </summary>
    [Range(MIN_CMV, MAX_CMV)]
    [JsonProperty(PropertyName = "cmvTarget", Required = Required.Default)]
    public short CmvTarget { get; protected set; }

    /// <summary>
    /// The maximum CMV value to be considered 'compacted' expressed in 10ths of units
    /// </summary>
    [Range(MIN_CMV, MAX_CMV)]
    [JsonProperty(PropertyName = "maxCMV", Required = Required.Default)]
    public short MaxCMV { get; protected set; }

    /// <summary>
    /// The maximum percentage the measured CMV may be compared to the cmvTarget from the machine, or the cmvTarget override if overrideTargetCMV is true
    /// </summary>
    [Range(MIN_PERCENT_CMV, MAX_PERCENT_CMV)]
    [JsonProperty(PropertyName = "maxCMVPercent", Required = Required.Default)]
    public double MaxCMVPercent { get; protected set; }

    /// <summary>
    /// The minimum CMV value to be considered 'compacted' expressed in 10ths of units
    /// </summary>
    [Range(MIN_CMV, MAX_CMV)]
    [JsonProperty(PropertyName = "minCMV", Required = Required.Default)]
    public short MinCMV { get; protected set; }

    /// <summary>
    /// The minimum percentage the measured CMV may be compared to the cmvTarget from the machine, or the cmvTarget override if overrideTargetCMV is true
    /// </summary>
    [Range(MIN_PERCENT_CMV, MAX_PERCENT_CMV)]
    [JsonProperty(PropertyName = "minCMVPercent", Required = Required.Default)]
    public double MinCMVPercent { get; protected set; }

    /// <summary>
    /// Override the target CMV recorded from the machine with the value of cmvTarget
    /// </summary>
    [JsonProperty(PropertyName = "overrideTargetCMV", Required = Required.Default)]
    [Required]
    public bool OverrideTargetCMV { get; protected set; }

    /// <summary>
    /// Default protected constructor
    /// </summary>
    protected CMVSettings()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="cmvTarget"></param>
    /// <param name="maxCMV"></param>
    /// <param name="maxCMVPercent"></param>
    /// <param name="minCMV"></param>
    /// <param name="minCMVPercent"></param>
    /// <param name="overrideTargetCMV"></param>
    public CMVSettings
    (
      short cmvTarget,
      short maxCMV,
      double maxCMVPercent,
      short minCMV,
      double minCMVPercent,
      bool overrideTargetCMV
    )
    {
      CmvTarget = cmvTarget;
      MaxCMV = maxCMV;
      MaxCMVPercent = maxCMVPercent;
      MinCMV = minCMV;
      MinCMVPercent = minCMVPercent;
      OverrideTargetCMV = overrideTargetCMV;
    }
    
    /// <summary>
    /// Validates all properties
    /// </summary>
    public virtual void Validate()
    {
      if (OverrideTargetCMV)
      {
        if (!(CmvTarget > 0) || !(MinCMV > 0 && MaxCMV > 0 || MinCMVPercent > 0 && MaxCMVPercent > 0))
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Invalid CMV settings: if overriding Target, Target and (CMV Percentage or CMV values) shall be specified."));
        }
      }

      if (CmvTarget > 0)
      {
        if (MinCMVPercent > 0 || MaxCMVPercent > 0)
        {
          ValidateRange(MinCMVPercent, MaxCMVPercent);
        }

        if (MinCMV > 0 || MaxCMV > 0)
        {
          ValidateRange(MinCMV, MaxCMV);
          ValidateRange(MinCMV, CmvTarget);
          ValidateRange(CmvTarget, MaxCMV);
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