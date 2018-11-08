using VSS.TRex.Analytics.SpeedStatistics.GridFabric;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Analytics.Responses
{
  public class ToFromBinary_SpeedStatisticsResponse : BaseTests
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
        CellSize = CELL_SIZE,
        CellsScannedOverTarget = CELLS_OVER_TARGET,
        CellsScannedAtTarget = CELLS_AT_TARGET,
        CellsScannedUnderTarget = CELLS_UNDER_TARGET,
        SummaryCellsScanned = CELLS_OVER_TARGET + CELLS_AT_TARGET + CELLS_UNDER_TARGET,
        IsTargetValueConstant = true,
        Counts = CountsArray,
        MissingTargetValue = false
      };

      SimpleBinarizableInstanceTester.TestClass(response, "Custom SpeedStatisticsResponse not same after round trip serialisation");
    }
  }
}
