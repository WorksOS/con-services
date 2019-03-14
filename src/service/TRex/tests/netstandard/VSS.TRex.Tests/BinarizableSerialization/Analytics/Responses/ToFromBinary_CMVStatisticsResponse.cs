using VSS.TRex.Analytics.CMVStatistics.GridFabric;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Analytics.Responses
{
  public class ToFromBinary_CMVStatisticsResponse
  {
    [Fact]
    public void Test_CMVStatisticsResponse_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<CMVStatisticsResponse>("Empty CMVStatisticsResponse not same after round trip serialisation");
    }

    [Fact]
    public void Test_CMVStatisticsResponse()
    {
      var response = new CMVStatisticsResponse()
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
        LastTargetCMV = 500
      };

      SimpleBinarizableInstanceTester.TestClass(response, "Custom CMVStatisticsResponse not same after round trip serialisation");
    }
  }
}
