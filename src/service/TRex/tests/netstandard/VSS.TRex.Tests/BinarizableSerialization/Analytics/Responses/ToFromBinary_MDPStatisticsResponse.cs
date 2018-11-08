using VSS.TRex.Analytics.MDPStatistics.GridFabric;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Analytics.Responses
{
  public class ToFromBinary_MDPStatisticsResponse : BaseTests
  {
    [Fact]
    public void Test_MDPStatisticsResponse_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<MDPStatisticsResponse>("Empty MDPStatisticsResponse not same after round trip serialisation");
    }

    [Fact]
    public void Test_MDPStatisticsResponse()
    {
      var response = new MDPStatisticsResponse()
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
        LastTargetMDP = 500
      };

      SimpleBinarizableInstanceTester.TestClass(response, "Custom MDPStatisticsResponse not same after round trip serialisation");
    }
  }
}
