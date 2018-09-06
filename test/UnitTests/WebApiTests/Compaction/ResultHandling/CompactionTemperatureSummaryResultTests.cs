using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.Compaction.ResultHandling
{
  [TestClass]
  public class CompactionTemperatureSummaryResultTests
  {
    [TestMethod]
    public void CreateTemperatureSummaryResult_Should_return_null_object_When_TotalAreaCoveredSqMeters_is_null()
    {
      var temperatureSummaryResult = new TemperatureSummaryResult(1, 2, false, 3, 0, 5, 6, 7);
      var result = new CompactionTemperatureSummaryResult(temperatureSummaryResult);

      Assert.IsNotNull(result);
      Assert.AreEqual(ContractExecutionResult.DefaultMessage, result.Message);
      Assert.IsNull(result.SummaryData);
    }

    [TestMethod]
    public void CreateTemperatureSummaryResult_Should_return_full_object_When_TotalAreaCoveredSqMeters_is_not_null()
    {
      var temperatureSummaryResult = new TemperatureSummaryResult(1, 2, false, 3, 3256.4, 4, 5, 6);
      var result = new CompactionTemperatureSummaryResult(temperatureSummaryResult);

      Assert.IsNotNull(result);
      Assert.AreEqual(ContractExecutionResult.DefaultMessage, result.Message);

      Assert.AreEqual(3256.4, result.SummaryData.TotalAreaCoveredSqMeters);
      Assert.AreEqual(5, result.SummaryData.PercentEqualsTarget);
      Assert.AreEqual(4, result.SummaryData.PercentGreaterThanTarget);
      Assert.AreEqual(6, result.SummaryData.PercentLessThanTarget);
      Assert.IsNotNull(result.SummaryData.TemperatureTarget);
      Assert.AreEqual(0.1, result.SummaryData.TemperatureTarget.MinTemperatureMachineTarget);
      Assert.AreEqual(0.2, result.SummaryData.TemperatureTarget.MaxTemperatureMachineTarget);
    }
  }
}
