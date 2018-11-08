using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// The request representation used to request Pass Count details.
  /// </summary>
  public class PassCountDetailsRequest : ProjectID
  {
    /// <summary>
    /// The filter instance to use in the request
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult Filter { get; private set; }

    /// <summary>
    /// The array of passcount numbers to be accounted for in the pass count analysis.
    /// Order is from low to high. There must be at least one item in the array and the first item's value must > 0. 
    /// If not supplied the default range is 1-8. 
    /// Please note you always get two extra elements returned in the output array. One at the beginning to respresent value 0 and the last
    /// element in the results array respresents the percentage of passes above your max value. 
    /// The values do not need to be evenly spaced but must increase. Any gap in number sequence results in
    /// accumulation of passcount results. e.g. array 2,5 results a result combining passcounts 2,3,4 totals.
    /// </summary>
    [JsonProperty(PropertyName = "passCounts", Required = Required.Default)]
    public int[] PassCounts { get; private set; }

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private PassCountDetailsRequest()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public PassCountDetailsRequest(
      Guid? projectUid,
      FilterResult filter,
      int[] passCounts
    )
    {
      ProjectUid = projectUid;
      Filter = filter;
      PassCounts = passCounts;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      Filter?.Validate();

      const ushort MIN_TARGET_PASS_COUNT = 0;
      const ushort MAX_TARGET_PASS_COUNT = ushort.MaxValue;

      if (PassCounts == null || PassCounts.Length == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Pass counts required"));
      }
      if (PassCounts[0] == MIN_TARGET_PASS_COUNT)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Pass counts must start greater than {MIN_TARGET_PASS_COUNT}"));
      }
      for (int i = 1; i < PassCounts.Length; i++)
      {
        if (PassCounts[i] <= PassCounts[i - 1])
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Pass counts must be ordered from lowest to the highest"));
        }
      }
      if (PassCounts[PassCounts.Length - 1] < MIN_TARGET_PASS_COUNT || PassCounts[PassCounts.Length - 1] > MAX_TARGET_PASS_COUNT)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Pass counts must be between {MIN_TARGET_PASS_COUNT + 1} and {MAX_TARGET_PASS_COUNT}"));
      }
    }
  }
}
