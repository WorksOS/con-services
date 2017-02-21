using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.ResultHandling;

namespace VSS.Raptor.Service.Common.Models
{
  /// <summary>
  /// The range of permissible temperatures to report on.
  /// </summary>
    public class TemperatureWarningLevels : IValidatable
  {
    /// <summary>
    /// The minimum permitted value in 10ths of a degree celcius. For example, 300 means 30.0°C.
    /// </summary>
    [Range(MIN_TEMPERATURE, MAX_TEMPERATURE)]
    [JsonProperty(PropertyName = "min", Required = Required.Always)]
    public ushort min { get; private set; }

    /// <summary>
    /// The maximum permitted value in 10ths of a degree celcius. For example, 800 means 80.0°C.
    /// </summary>
    [Range(MIN_TEMPERATURE, MAX_TEMPERATURE)]
    [JsonProperty(PropertyName = "max", Required = Required.Always)]
    public ushort max { get; private set; }


    /// <summary>
    /// Private constructor
    /// </summary>
    private TemperatureWarningLevels()
    {}

    /// <summary>
    /// Create instance of TemperatureWarningLevels
    /// </summary>
    public static TemperatureWarningLevels CreateTemperatureWarningLevels
        (
          ushort min,
          ushort max
        )
    {
      return new TemperatureWarningLevels
             {
               min = min,
               max = max
             };
    }

    /// <summary>
    /// Create example instance of TemperatureWarningLevels to display in Help documentation.
    /// </summary>
    public static TemperatureWarningLevels HelpSample
    {
      get
      {
        return new TemperatureWarningLevels()
        {
          min = 300,
          max = 800
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
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Temperature warning level minimum must be less than Temperature warning level maximum"));
      }      
    }

    private const ushort MIN_TEMPERATURE = 0;
    private const ushort MAX_TEMPERATURE = 4095;//10ths degrees Celcius i.e. 409.5°C


  }
}