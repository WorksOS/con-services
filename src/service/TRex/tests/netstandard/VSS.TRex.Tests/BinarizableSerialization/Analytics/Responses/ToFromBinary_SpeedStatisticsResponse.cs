using VSS.TRex.Analytics.SpeedStatistics.GridFabric;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Analytics.Responses
{
  public class ToFromBinary_SpeedStatisticsResponse
  {
    [Fact]
    public void Test_SpeedStatisticsResponse_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<SpeedStatisticsResponse>("Empty SpeedStatisticsResponse not same after round trip serialisation");
    }

    [Fact]
    public void Test_SpeedStatisticsResponse()
    {
      var response = new SpeedStatisticsResponse()
      {
        ResultStatus = RequestErrorStatus.OK,
        CellSize = TestConsts.CELL_SIZE,
        CellsScannedOverTarget = TestConsts.CELLS_OVER_TARGET,
        CellsScannedAtTarget = TestConsts.CELLS_AT_TARGET,
        CellsScannedUnderTarget = TestConsts.CELLS_UNDER_TARGET,
        SummaryCellsScanned = TestConsts.CELLS_OVER_TARGET + TestConsts.CELLS_AT_TARGET + TestConsts.CELLS_UNDER_TARGET,
        IsTargetValueConstant = true,
        Counts = TestConsts.CountsArray,
        MissingTargetValue = false
      };

      SimpleBinarizableInstanceTester.TestClass(response, "Custom SpeedStatisticsResponse not same after round trip serialisation");
    }
  }
}
