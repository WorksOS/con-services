using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// Contains a percentage range of observed MDP values with respect to the target MDP value configured on a machine
  /// </summary>
  public class MDPRangePercentage 
  {
    /// <summary>
    /// The minimum percentage range. Must be between 0 and 250.
    /// </summary>
    [Range(MIN_PERCENT, MAX_PERCENT)]
    [JsonProperty(PropertyName = "min", Required = Required.Always)]
    [Required]
    public double min { get; private set; }

    /// <summary>
    /// The maximum percentage range. Must be between 0 and 250.
    /// </summary>
    [Range(MIN_PERCENT, MAX_PERCENT)]
    [JsonProperty(PropertyName = "max", Required = Required.Always)]
    [Required]
    public double max { get; private set; }

    
    /// <summary>
    /// Deafault private constructor
    /// </summary>
    private MDPRangePercentage()
    {}

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public MDPRangePercentage(double min, double max)
    {
      this.min = min;
      this.max = max;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (min > max)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "MDP percentage minimum must be less than MDP percentage maximum"));
      }
    }

    private const double MIN_PERCENT = 0.0;
    private const double MAX_PERCENT = 250.0;
  }
}