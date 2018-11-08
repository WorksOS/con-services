using VSS.TRex.Analytics.TemperatureStatistics.GridFabric;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Analytics.Responses
{
  public class ToFromBinary_TemperatureStatisticsResponse : BaseTests
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
        CellSize = CELL_SIZE,
        CellsScannedOverTarget = CELLS_OVER_TARGET,
        CellsScannedAtTarget = CELLS_AT_TARGET,
        CellsScannedUnderTarget = CELLS_UNDER_TARGET,
        SummaryCellsScanned = CELLS_OVER_TARGET + CELLS_AT_TARGET + CELLS_UNDER_TARGET,
        IsTargetValueConstant = true,
        Counts = CountsArray,
        MissingTargetValue = false,
        LastTempRangeMin = 300,
        LastTempRangeMax = 1500
      };

      SimpleBinarizableInstanceTester.TestClass(response, "Custom TemperatureStatisticsResponse not same after round trip serialisation");
    }
  }
}
