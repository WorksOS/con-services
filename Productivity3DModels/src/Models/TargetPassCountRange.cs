using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// Contains a range of Target Pass Count values.
  /// </summary>
  public class TargetPassCountRange 
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
    /// Defauilt private constructor.
    /// </summary>
    private TargetPassCountRange()
    {
      // ...
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public TargetPassCountRange(ushort min, ushort max)
    {
      this.min = min;
      this.max = max;
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