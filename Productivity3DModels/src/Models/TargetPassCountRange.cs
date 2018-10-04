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
    private const ushort MIN_TARGET_PASS_COUNT = 1;
    private const ushort MAX_TARGET_PASS_COUNT = ushort.MaxValue; // 65535...

    /// <summary>
    /// The minimum range value. Must be between 1 and 65535.
    /// </summary>
    [Range(MIN_TARGET_PASS_COUNT, MAX_TARGET_PASS_COUNT)]
    [JsonProperty(PropertyName = "min", Required = Required.Always)]
    [Required]
    public ushort Min { get; private set; }

    /// <summary>
    /// The maximum range value. Must be between 1 and 65535.
    /// </summary>
    [Range(MIN_TARGET_PASS_COUNT, MAX_TARGET_PASS_COUNT)]
    [JsonProperty(PropertyName = "max", Required = Required.Always)]
    [Required]
    public ushort Max { get; private set; }

    
    /// <summary>
    /// Default private constructor.
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
      Min = min;
      Max = max;
    }

    /// <summary>
    /// Validates all properties.
    /// </summary>
    public void Validate()
    {

      if (Min > Max)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Target Pass Count minimum value must be less than Target Pass Count maximum value."));      
      }
    }
  }
}