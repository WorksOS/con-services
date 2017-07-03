using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.Common.Models
{
  /// <summary>
  /// Contains a range of Target Pass Count values.
  /// </summary>
  /// 
  public class TargetPassCountRange : IValidatable
  {
    /// <summary>
    /// The minimum range value. Must be between 1 and 65535.
    /// </summary>
    [Range(MIN_TARGET_PASS_COUNT, MAX_TARGET_PASS_COUNT)]
    [JsonProperty(PropertyName = "min", Required = Required.Always)]
    [Required]
    public ushort min { get; private set; }

    /// <summary>
    /// The maximum range value. Must be between 1 and 65535.
    /// </summary>
    [Range(MIN_TARGET_PASS_COUNT, MAX_TARGET_PASS_COUNT)]
    [JsonProperty(PropertyName = "max", Required = Required.Always)]
    [Required]
    public ushort max { get; private set; }

    
    /// <summary>
    /// Private constructor.
    /// </summary>
    private TargetPassCountRange()
    {
      // ...
    }

    /// <summary>
    /// Create an instance of the TargetPassCountRange class.
    /// </summary>
    public static TargetPassCountRange CreateTargetPassCountRange
        (
          ushort min,
          ushort max
        )
    {
      return new TargetPassCountRange
      {
        min = min,
        max = max
      };
    }

    /// <summary>
    /// Create a sample instance of TargetPassCountRange class to display in Help documentation.
    /// </summary>
    public static TargetPassCountRange HelpSample
    {
      get
      {
        return new TargetPassCountRange
        {
          min = 2,
          max = 12
        };
      }
    }


    /// <summary>
    /// Validates all properties.
    /// </summary>
    public void Validate()
    {

      if (min > max)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Target Pass Count minimum value must be less than Target Pass Count maximum value."));      
      }
    }

    private const ushort MIN_TARGET_PASS_COUNT = 1;
    private const ushort MAX_TARGET_PASS_COUNT = ushort.MaxValue; // 65535...
  }
}
