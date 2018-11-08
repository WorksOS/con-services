using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  /// <summary>
  /// Settings values for cut-fill queries
  /// </summary>
  public class CutFillSettings : IValidatable
  {
    [JsonProperty(PropertyName = "percents", Required = Required.Default)]
    public double[] percents { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private CutFillSettings()
    {
    }

    /// <summary>
    /// Create instance of CutFillSettings
    /// </summary>
    public static CutFillSettings CreateCutFillSettings(
      double[] percents
    )
    {
      return new CutFillSettings
      {
        percents = percents,
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      const double MIN_CUT_FILL = -400.0; //m
      const double MAX_CUT_FILL = 400.0;
      const int CUT_FILL_TOTAL = 7;

      if (percents.Length != CUT_FILL_TOTAL)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Exactly {CUT_FILL_TOTAL} cut-fill tolerances must be specified"));
      }

      for (int i = 0; i < CUT_FILL_TOTAL; i++)
      {
        if (percents[i] < MIN_CUT_FILL || percents[i] > MAX_CUT_FILL)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              $"Cut-fill tolerances must be between {MIN_CUT_FILL} and {MAX_CUT_FILL} meters"));
        }
      }

      for (int i = 1; i < CUT_FILL_TOTAL; i++)
      {
        if (percents[i - 1] < percents[i])
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Cut-fill tolerances must be in order of highest cut to lowest fill"));
        }
      }

      if (percents[CUT_FILL_TOTAL / 2] != 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "On grade cut-fill tolerance must be 0"));
      }
    }
  }
}
