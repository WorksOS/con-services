using VSS.TRex.Analytics.PassCountStatistics.GridFabric;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Analytics.Responses
{
  public class ToFromBinary_PassCountStatisticsResponse : BaseTests
  {
    [Fact]
    public void Test_PassCountStatisticsResponse_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<PassCountStatisticsResponse>("Empty PassCountStatisticsResponse not same after round trip serialisation");
    }

    [Fact]
    public void Test_PassCountStatisticsResponse()
    {
      var response = new PassCountStatisticsResponse()
      {
        ResultStatus = RequestErrorStatus.OK,
        CellSize = CELL_SIZE,
        CellsScannedOverTarget = CELLS_OVER_TARGET,
        CellsScannedAtTarget = CELLS_AT_TARGET,
        CellsScannedUnderTarget = CELLS_UNDER_TARGET,
        SummaryCellsScanned = CELLS_OVER_TARGET + CELLS_AT_TARGET + CELLS_UNDER_TARGET,
        IsTargetValueConstant = true,
        Counts = CountsArray,
        MissingTargetValue = false,
        LastPassCountTargetRange = new PassCountRangeRecord(2, 9)
      };

      SimpleBinarizableInstanceTester.TestClass(response, "Custom PassCountStatisticsResponse not same after round trip serialisation");
    }
  }
}
