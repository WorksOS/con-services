using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// The parameters for Temperature detailed and summary computations
  /// </summary>
  public class TemperatureSettings //: IValidatable
  {
    private const double MIN_TEMPERATURE = 0;
    private const double MAX_TEMPERATURE = 4095;

    /// <summary>
    /// The maximum Temperature value in degrees Celsius
    /// </summary>
    [Range(MIN_TEMPERATURE, MAX_TEMPERATURE)]
    [JsonProperty(PropertyName = "maxTemperature", Required = Required.Default)]
    public double MaxTemperature { get; private set; }

    /// <summary>
    /// The minimum Temperature value in degrees Celsius
    /// </summary>
    [Range(MIN_TEMPERATURE, MAX_TEMPERATURE)]
    [JsonProperty(PropertyName = "minTemperature", Required = Required.Default)]
    public double MinTemperature { get; private set; }

    /// <summary>
    /// Override the target Temperature range recorded from the machine with the value of minTemperature to maxTemperature
    /// </summary>
    [JsonProperty(PropertyName = "overrideTemperatureRange", Required = Required.Always)]
    [Required]
    public bool OverrideTemperatureRange { get; private set; }

    /// <summary>
    /// Default private constructor
    /// </summary>
    private TemperatureSettings()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="maxTemperature"></param>
    /// <param name="minTemperature"></param>
    /// <param name="overrideTemperatureRange"></param>
    public TemperatureSettings(
        double maxTemperature,
        double minTemperature, 
        bool overrideTemperatureRange
        )
    {
      MaxTemperature = maxTemperature;
      MinTemperature = minTemperature;
      OverrideTemperatureRange = overrideTemperatureRange;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (OverrideTemperatureRange)
        if (!(MinTemperature > 0 && MaxTemperature > 0))
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
                new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Invalid Temperature settings: if overriding Target, Target and Temperature values shall be specified."));
        }

        if (MinTemperature > 0 || MaxTemperature > 0)
        {
          if (MinTemperature > MaxTemperature)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
                  new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Invalid Temperature settings values: must have minimum < maximum"));
          }
        }
    }
  }
}
