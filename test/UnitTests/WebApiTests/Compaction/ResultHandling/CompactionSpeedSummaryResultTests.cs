using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.Compaction.ResultHandling
{
  [TestClass]
  public class CompactionSpeedSummaryResultTests
  {
    [TestMethod]
    public void CreateSpeedSummaryResult_Should_return_empty_object_When_CoverageArea_is_null()
    {
      var summarySpeedResult = SummarySpeedResult.CreateSummarySpeedResult(1, 2, 3, 0);
      var machineSpeedTarget = MachineSpeedTarget.CreateMachineSpeedTarget(5, 6);
      var result = CompactionSpeedSummaryResult.CreateSpeedSummaryResult(summarySpeedResult, machineSpeedTarget);

      Assert.IsNotNull(result);
      Assert.AreEqual(ContractExecutionResult.DefaultMessage, result.Message);

      Assert.AreEqual(0, result.SummaryData.PercentGreaterThanTarget);
      Assert.AreEqual(0, result.SummaryData.PercentLessThanTarget);
      Assert.AreEqual(0, result.SummaryData.PercentEqualsTarget);
      Assert.AreEqual(0, result.SummaryData.TotalAreaCoveredSqMeters);
      Assert.AreEqual(0, result.SummaryData.MinTargetMachineSpeed);
      Assert.AreEqual(0, result.SummaryData.MaxTargetMachineSpeed);
    }

    [TestMethod]
    public void CreateSpeedSummaryResult_Should_return_full_object_When_CoverageArea_is_not_null()
    {
      var summarySpeedResult = SummarySpeedResult.CreateSummarySpeedResult(1, 2, 3, 8976.4);
      var machineSpeedTarget = MachineSpeedTarget.CreateMachineSpeedTarget(4, 5);
      var result = CompactionSpeedSummaryResult.CreateSpeedSummaryResult(summarySpeedResult, machineSpeedTarget);

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