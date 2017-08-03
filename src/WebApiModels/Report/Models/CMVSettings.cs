using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Interfaces;

namespace VSS.Productivity3D.WebApiModels.Report.Models
{
  /// <summary>
  /// The parameters for CMV detailed and summary computations
  /// </summary>
  public class CMVSettings : IValidatable
  {
    /// <summary>
    /// The target CMV value expressed in 10ths of units
    /// </summary>
    [Range(MIN_CMV, MAX_CMV)]
    [JsonProperty(PropertyName = "cmvTarget", Required = Required.Default)]
    public short cmvTarget { get; private set; }

    /// <summary>
    /// The maximum CMV value to be considered 'compacted' expressed in 10ths of units
    /// </summary>
    [Range(MIN_CMV, MAX_CMV)]
    [JsonProperty(PropertyName = "maxCMV", Required = Required.Default)]
    public short maxCMV { get; private set; }

    /// <summary>
    /// The maximum percentage the measured CMV may be compared to the cmvTarget from the machine, or the cmvTarget override if overrideTargetCMV is true
    /// </summary>
    [Range(MIN_PERCENT_CMV, MAX_PERCENT_CMV)]
    [JsonProperty(PropertyName = "maxCMVPercent", Required = Required.Default)]
    public double maxCMVPercent { get; private set; }

    /// <summary>
    /// The minimum CMV value to be considered 'compacted' expressed in 10ths of units
    /// </summary>
    [Range(MIN_CMV, MAX_CMV)]
    [JsonProperty(PropertyName = "minCMV", Required = Required.Default)]
    public short minCMV { get; private set; }

    /// <summary>
    /// The minimum percentage the measured CMV may be compared to the cmvTarget from the machine, or the cmvTarget override if overrideTargetCMV is true
    /// </summary>
    [Range(MIN_PERCENT_CMV, MAX_PERCENT_CMV)]
    [JsonProperty(PropertyName = "minCMVPercent", Required = Required.Default)]
    public double minCMVPercent { get; private set; }

    /// <summary>
    /// Override the target CMV recorded from the machine with the value of cmvTarget
    /// </summary>
    [JsonProperty(PropertyName = "overrideTargetCMV", Required = Required.Always)]
    [Required]
    public bool overrideTargetCMV { get; private set; }

    private const short MIN_CMV = 0;
    private const short MAX_CMV = 10000;
    private const double MIN_PERCENT_CMV = 0.0;
    private const double MAX_PERCENT_CMV = 250.0;

    /// <summary>
    /// Private constructor
    /// </summary>
    private CMVSettings()
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
    /// Create example instance of CMVSettings to display in Help documentation.
    /// </summary>
    public static CMVSettings HelpSample => new CMVSettings
    {
      cmvTarget = 400,
      maxCMV = 800,
      maxCMVPercent = 130.0,
      minCMV = 0,
      minCMVPercent = 0.0,
      overrideTargetCMV = true
    };


    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (overrideTargetCMV)
        if (!(cmvTarget > 0) || !(minCMV > 0 && maxCMV > 0 || minCMVPercent > 0 && maxCMVPercent > 0))
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
                new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Invalid CMV settings: if overriding Target, Target and (CMV Percentage or CMV values) shall be specified."));
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

    private void ValidateRange(double lower, double upper)
    {
      if (lower > upper)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Invalid CMV settings values: must have minimum < target < maximum and minimum % < maximum %"));
      }
    }
  }
}