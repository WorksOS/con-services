using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.ResultHandling;

namespace VSS.Raptor.Service.Common.Models
{
  /// <summary>
  /// Contains a percentage range of observed MDP values with respect to the target MDP value configured on a machine
  /// </summary>
    public class MDPRangePercentage : IValidatable
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
    /// Private constructor
    /// </summary>
    private MDPRangePercentage()
    {}

    /// <summary>
    /// Create instance of MDPRangePercentage
    /// </summary>
    public static MDPRangePercentage CreateMdpRangePercentage
        (
          double min,
          double max
        )
    {
      return new MDPRangePercentage
             {
               min = min,
               max = max
             };
    }

    /// <summary>
    /// Create example instance of MDPRangePercentage to display in Help documentation.
    /// </summary>
    public static MDPRangePercentage HelpSample
    {
      get
      {
        return new MDPRangePercentage()
        {
          min = 80.0,
          max = 130.0
        };
      }
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (this.min > this.max)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "MDP percentage minimum must be less than MDP percentage maximum"));
      }
    }

    private const double MIN_PERCENT = 0.0;
    private const double MAX_PERCENT = 250.0;


  }
}