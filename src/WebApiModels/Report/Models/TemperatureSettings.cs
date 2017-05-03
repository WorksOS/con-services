
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.ResultHandling;

namespace VSS.Raptor.Service.WebApiModels.Report.Models
{
  /// <summary>
  /// The parameters for Temperature detailed and summary computations
  /// </summary>
  public class TemperatureSettings : IValidatable
  {
    /// <summary>
    /// The maximum Temperature value 
    /// </summary>
    [Range(MIN_TEMPERATURE, MAX_TEMPERATURE)]
    [JsonProperty(PropertyName = "maxTemperature", Required = Required.Default)]
    public double maxTemperature { get; private set; }

    /// <summary>
    /// The minimum Temperature value 
    /// </summary>
    [Range(MIN_TEMPERATURE, MAX_TEMPERATURE)]
    [JsonProperty(PropertyName = "minTemperature", Required = Required.Default)]
    public double minTemperature { get; private set; }

    /// <summary>
    /// Override the target Temperature range recorded from the machine with the value of minTemperature to maxTemperature
    /// </summary>
    [JsonProperty(PropertyName = "overrideTemperatureRange", Required = Required.Always)]
    [Required]
    public bool overrideTemperatureRange { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private TemperatureSettings()
    {
    }

    /// <summary>
    /// Create instance of TemperatureSettings
    /// </summary>
    public static TemperatureSettings CreateTemperatureSettings(
        short maxTemperature,
        short minTemperature, 
        bool overrideTemperatureRange
        )
    {
      return new TemperatureSettings
      {
        maxTemperature = maxTemperature,
        minTemperature = minTemperature,
        overrideTemperatureRange = overrideTemperatureRange
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (overrideTemperatureRange)
        if (!(minTemperature > 0 && maxTemperature > 0))
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
                new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Invalid Temperature settings: if overriding Target, Target and Temperature values shall be specified."));
        }

        if (minTemperature > 0 || maxTemperature > 0)
        {
          if (minTemperature > maxTemperature)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
                  new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Invalid Temperature settings values: must have minimum < maximum"));
          }
        }
    }

    private const double MIN_TEMPERATURE = 0;
    private const double MAX_TEMPERATURE = 4095;

  }
}
