using VSS.TRex.Analytics.CutFillStatistics.GridFabric;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Analytics.Responses
{
  public class ToFromBinary_CutFillStatisticsResponse : BaseTests
  {
    [Fact]
    public void Test_CutFillStatisticsResponse_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<CutFillStatisticsResponse>("Empty CutFillStatisticsResponse not same after round trip serialisation");
    }

    [Fact]
    public void Test_CutFillStatisticsResponse()
    {
      var response = new CutFillStatisticsResponse()
      {
        ResultStatus = RequestErrorStatus.OK,
        CellSize = CELL_SIZE,
        CellsScannedOverTarget = CELLS_OVER_TARGET,
        CellsScannedAtTarget = CELLS_AT_TARGET,
        CellsScannedUnderTarget = CELLS_UNDER_TARGET,
        SummaryCellsScanned = CELLS_OVER_TARGET + CELLS_AT_TARGET + CELLS_UNDER_TARGET,
        IsTargetValueConstant = true,
        Counts = CountsArray,
        MissingTargetValue = false
      };

      SimpleBinarizableInstanceTester.TestClass(response, "Custom CutFillStatisticsResponse not same after round trip serialisation");
    }
  }
}
