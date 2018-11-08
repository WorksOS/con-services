using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  /// <summary>
  /// Settings values for CMV % change queries
  /// </summary>
  public class CmvPercentChangeSettings : IValidatable
  {
    [JsonProperty(PropertyName = "percents", Required = Required.Default)]
    public double[] percents { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private CmvPercentChangeSettings()
    {
    }

    /// <summary>
    /// Create instance of CmvPercentChangeSettings
    /// </summary>
    public static CmvPercentChangeSettings CreateCmvPercentChangeSettings(
      double[] percents
    )
    {
      return new CmvPercentChangeSettings
      {
        percents = percents,
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      const double MIN_PERCENT_CHANGE = 0;
      const double MAX_PERCENT_CHANGE = 100;

      if (percents == null || percents.Length == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Percents required"));
      }
      if (percents[0] <= MIN_PERCENT_CHANGE)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Percents must start greater than {MIN_PERCENT_CHANGE}"));
      }
      for (int i = 1; i < percents.Length; i++)
      {
        if (percents[i] <= percents[i - 1])
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Percents must be ordered from lowest to the highest"));
        }
      }
      if (percents[percents.Length - 1] < MIN_PERCENT_CHANGE || percents[percents.Length - 1] > MAX_PERCENT_CHANGE)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Percents must be between {MIN_PERCENT_CHANGE + 1} and {MAX_PERCENT_CHANGE}"));
      }
    }
  }
}
