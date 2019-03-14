using VSS.TRex.Analytics.TemperatureStatistics.GridFabric;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Analytics.Responses
{
  public class ToFromBinary_TemperatureStatisticsResponse
  {
    [Fact]
    public void Test_TemperatureStatisticsResponse_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<TemperatureStatisticsResponse>("Empty TemperatureStatisticsResponse not same after round trip serialisation");
    }

    [Fact]
    public void Test_TemperatureStatisticsResponse()
    {
      var response = new TemperatureStatisticsResponse()
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
        LastTempRangeMin = 300,
        LastTempRangeMax = 1500
      };

      SimpleBinarizableInstanceTester.TestClass(response, "Custom TemperatureStatisticsResponse not same after round trip serialisation");
    }
  }
}
