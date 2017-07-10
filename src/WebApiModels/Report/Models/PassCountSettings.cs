using Newtonsoft.Json;
using System.Net;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Report.Models
{
  /// <summary>
  /// Setting and configuration values related to processing pass count related queries
  /// </summary>
  public class PassCountSettings : IValidatable
  {
    /// <summary>
    /// The array of passcount numbers to be accounted for in the pass count analysis.
    /// Order is from low to high. There must be at least one item in the array and the first item's value must > 0. 
    /// If not supplied the default range is 1-8. 
    /// Please note you always get two extra elements returned in the output array. One at the beginning to respresent value 0 and the last
    /// element in the results array respresents the percentage of passes above your max value. 
    /// The values do not need to be evenly spaced but must increase. Any gap in number sequence results in
    /// accumulation of passcount results. e.g. array 2,5 results a result combining passcounts 2,3,4 totals. 
    /// This property is not used for a summary report only for a detailed report.
    /// </summary>
    [JsonProperty(PropertyName = "passCounts", Required = Required.Default)]
    public int[] passCounts { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private PassCountSettings()
    {
    }

    /// <summary>
    /// Create instance of PassCountSettings
    /// </summary>
    public static PassCountSettings CreatePassCountSettings(
        int[] passCounts
        )
    {
      return new PassCountSettings
      {
        passCounts = passCounts,
      };
    }

    /// <summary>
    /// Create example instance of PassCountSettings to display in Help documentation.
    /// </summary>
    public static PassCountSettings HelpSample
    {
      get
      {
        return new PassCountSettings
        {
          passCounts = new int[] { 1, 2, 3, 5, 8, 12, 20 }
        };
      }
    }


    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      const ushort MIN_TARGET_PASS_COUNT = 0;
      const ushort MAX_TARGET_PASS_COUNT = ushort.MaxValue;

      if (passCounts == null || passCounts.Length == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Pass counts required"));
      }
      if (passCounts[0] == MIN_TARGET_PASS_COUNT)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                $"Pass counts must start greater than {MIN_TARGET_PASS_COUNT}"));
      }
      for (int i = 1; i < passCounts.Length; i++)
      {
        if (passCounts[i] <= passCounts[i - 1])
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
                new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Pass counts must be ordered from lowest to the highest"));
        }
      }
      if (passCounts[passCounts.Length - 1] < MIN_TARGET_PASS_COUNT || passCounts[passCounts.Length - 1] > MAX_TARGET_PASS_COUNT)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                $"Pass counts must be between {MIN_TARGET_PASS_COUNT + 1} and {MAX_TARGET_PASS_COUNT}"));
      }
    }
  }
}