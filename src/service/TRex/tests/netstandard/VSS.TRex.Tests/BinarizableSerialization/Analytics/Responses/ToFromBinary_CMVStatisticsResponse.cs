using VSS.TRex.Analytics.CMVStatistics.GridFabric;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Analytics.Responses
{
  public class ToFromBinary_CMVStatisticsResponse : BaseTests
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
        CellSize = CELL_SIZE,
        CellsScannedOverTarget = CELLS_OVER_TARGET,
        CellsScannedAtTarget = CELLS_AT_TARGET,
        CellsScannedUnderTarget = CELLS_UNDER_TARGET,
        SummaryCellsScanned = CELLS_OVER_TARGET + CELLS_AT_TARGET + CELLS_UNDER_TARGET,
        IsTargetValueConstant = true,
        Counts = CountsArray,
        MissingTargetValue = false,
        LastTargetCMV = 500
      };

      SimpleBinarizableInstanceTester.TestClass(response, "Custom CMVStatisticsResponse not same after round trip serialisation");
    }
  }
}
