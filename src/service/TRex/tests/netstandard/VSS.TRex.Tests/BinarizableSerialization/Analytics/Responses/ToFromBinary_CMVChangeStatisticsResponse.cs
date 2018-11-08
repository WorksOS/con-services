using VSS.TRex.Analytics.CMVChangeStatistics.GridFabric;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Analytics.Responses
{
  public class ToFromBinary_CMVChangeStatisticsResponse : BaseTests
  {
    [Fact]
    public void Test_CMVChangeStatisticsResponse_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<CMVChangeStatisticsResponse>("Empty CMVChangeStatisticsResponse not same after round trip serialisation");
    }

    [Fact]
    public void Test_CMVChangeStatisticsResponse()
    {
      var response = new CMVChangeStatisticsResponse()
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

      SimpleBinarizableInstanceTester.TestClass(response, "Custom CMVChangeStatisticsResponse not same after round trip serialisation");
    }
  }
}
