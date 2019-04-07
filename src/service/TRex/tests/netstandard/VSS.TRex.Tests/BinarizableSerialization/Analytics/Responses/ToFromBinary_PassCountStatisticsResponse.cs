using VSS.TRex.Analytics.PassCountStatistics.GridFabric;
using VSS.TRex.Common.Records;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Analytics.Responses
{
  public class ToFromBinary_PassCountStatisticsResponse
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
        CellSize = TestConsts.CELL_SIZE,
        CellsScannedOverTarget = TestConsts.CELLS_OVER_TARGET,
        CellsScannedAtTarget = TestConsts.CELLS_AT_TARGET,
        CellsScannedUnderTarget = TestConsts.CELLS_UNDER_TARGET,
        SummaryCellsScanned = TestConsts.CELLS_OVER_TARGET + TestConsts.CELLS_AT_TARGET + TestConsts.CELLS_UNDER_TARGET,
        IsTargetValueConstant = true,
        Counts = TestConsts.CountsArray,
        MissingTargetValue = false,
        LastPassCountTargetRange = new PassCountRangeRecord(2, 9)
      };

      SimpleBinarizableInstanceTester.TestClass(response, "Custom PassCountStatisticsResponse not same after round trip serialisation");
    }
  }
}
