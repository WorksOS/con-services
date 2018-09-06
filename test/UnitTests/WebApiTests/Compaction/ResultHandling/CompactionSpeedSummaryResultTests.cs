using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.Compaction.ResultHandling
{
  [TestClass]
  public class CompactionSpeedSummaryResultTests
  {
    [TestMethod]
    public void CreateSpeedSummaryResult_Should_return_null_object_When_CoverageArea_is_null()
    {
      var summarySpeedResult = new SpeedSummaryResult(1, 2, 3, 0);
      var machineSpeedTarget = new MachineSpeedTarget(5, 6);
      var result = new CompactionSpeedSummaryResult(summarySpeedResult, machineSpeedTarget);

      Assert.IsNotNull(result);
      Assert.AreEqual(ContractExecutionResult.DefaultMessage, result.Message);
      Assert.IsNull(result.SummaryData);
    }

    [TestMethod]
    public void CreateSpeedSummaryResult_Should_return_full_object_When_CoverageArea_is_not_null()
    {
      var summarySpeedResult = new SpeedSummaryResult(1, 2, 3, 8976.4);
      var machineSpeedTarget = new MachineSpeedTarget(4, 5);
      var result = new CompactionSpeedSummaryResult(summarySpeedResult, machineSpeedTarget);

      Assert.IsNotNull(result);
      Assert.AreEqual(ContractExecutionResult.DefaultMessage, result.Message);

      Assert.AreEqual(1, result.SummaryData.PercentGreaterThanTarget);
      Assert.AreEqual(2, result.SummaryData.PercentLessThanTarget);
      Assert.AreEqual(3, result.SummaryData.PercentEqualsTarget);
      Assert.AreEqual(8976.4, result.SummaryData.TotalAreaCoveredSqMeters);
      Assert.AreEqual(0.1, result.SummaryData.MinTargetMachineSpeed);
      Assert.AreEqual(0.2, result.SummaryData.MaxTargetMachineSpeed);
    }
  }
}
