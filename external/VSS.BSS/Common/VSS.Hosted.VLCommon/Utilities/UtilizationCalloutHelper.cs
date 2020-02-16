using System.Collections.Generic;
using System.Linq;
using VSS.Hosted.VLCommon.Resources;

namespace VSS.Hosted.VLCommon.Utilities
{
  public static class UtilizationCalloutHelper
  {
    private static readonly List<string> _dateRangeCalloutTokens = new List<string>
    {
      "none",
      "dateRangeCalloutMissingMeterValue",
      "dateRangeCalloutMultipleDayDelta",
      "dateRangeCalloutSpike",
      "notApplicable",
      "dateRangeCalloutNegativeValue"
    };

    private static readonly List<string> _singleDayCalloutTokens = new List<string>
    {
      "none",
      "singleDayCalloutMissingMeterValue",
      "singleDayCalloutMultipleDayDelta",
      "singleDayCalloutSpike",
      "notApplicable",
      "singleDayCalloutNegativeValue"
    };
    public static bool HasUtilizationCallouts(List<int> calloutTypeIDs)
    {
      return calloutTypeIDs.Count > 0;
    }
  }
}
