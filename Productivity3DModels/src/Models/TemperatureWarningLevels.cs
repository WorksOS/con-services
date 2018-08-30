using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// The range of permissible temperatures to report on.
  /// </summary>
  public class TemperatureWarningLevels 
  {
    /// <summary>
    /// The minimum permitted value in 10ths of a degree celcius. For example, 300 means 30.0°C.
    /// </summary>
    [Range(MIN_TEMPERATURE, MAX_TEMPERATURE)]
    [JsonProperty(PropertyName = "min", Required = Required.Always)] 
    [Required]
    public ushort min { get; private set; }

    /// <summary>
    /// The maximum permitted value in 10ths of a degree celcius. For example, 800 means 80.0°C.
    /// </summary>
    [Range(MIN_TEMPERATURE, MAX_TEMPERATURE)]
    [JsonProperty(PropertyName = "max", Required = Required.Always)]
    [Required]
    public ushort max { get; private set; }


    /// <summary>
    /// Private constructor
    /// </summary>
    private TemperatureWarningLevels()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public TemperatureWarningLevels(ushort min, ushort max)
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
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Temperature warning level minimum must be less than Temperature warning level maximum"));
      }
    }

    private const ushort MIN_TEMPERATURE = 0;
    private const ushort MAX_TEMPERATURE = 4095;//10ths degrees Celcius i.e. 409.5°C
  }
}