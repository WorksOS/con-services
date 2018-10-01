using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Models.Utilities;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  /// <summary>
  /// Settings values for temperature details queries
  /// </summary>
  public class TemperatureDetailsSettings
  {
    /// <summary>
    /// Custom target values in °C
    /// </summary>
    [JsonProperty(PropertyName = "customTemperatureDetailsTargets", Required = Required.Default)]
    public double[] CustomTemperatureDetailsTargets { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private TemperatureDetailsSettings()
    {
    }

    /// <summary>
    /// Create instance of CmvPercentChangeSettings
    /// </summary>
    public static TemperatureDetailsSettings Create(
      double[] targets
    )
    {
      return new TemperatureDetailsSettings
      {
        CustomTemperatureDetailsTargets = targets,
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      // Validate custom temperature Detail targets...
      if (CustomTemperatureDetailsTargets == null || CustomTemperatureDetailsTargets.Length == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Temperature Detail targets required"));
      }
      if (CustomTemperatureDetailsTargets[0] != ValidationConstants.MIN_TEMPERATURE)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Temperature Detail targets must start at {ValidationConstants.MIN_TEMPERATURE}"));
      }
      for (int i = 1; i < CustomTemperatureDetailsTargets.Length; i++)
      {
        if (CustomTemperatureDetailsTargets[i] <= CustomTemperatureDetailsTargets[i - 1])
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "CMV Detail targets must be ordered from lowest to the highest"));
        }
      }
      if (CustomTemperatureDetailsTargets[CustomTemperatureDetailsTargets.Length - 1] < ValidationConstants.MIN_TEMPERATURE || CustomTemperatureDetailsTargets[CustomTemperatureDetailsTargets.Length - 1] > ValidationConstants.MAX_TEMPERATURE)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Temperature Detail targets must be between {ValidationConstants.MIN_TEMPERATURE} and {ValidationConstants.MAX_TEMPERATURE}"));
      }
    }
  }
}
