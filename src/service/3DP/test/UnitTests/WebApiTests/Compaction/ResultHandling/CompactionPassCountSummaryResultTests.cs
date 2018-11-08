using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.Compaction.ResultHandling
{
  [TestClass]
  public class CompactionPassCountSummaryResultTests
  {
    [TestMethod]
    public void CreatePassCountSummaryResult_Should_return_null_object_When_TotalAreaCoveredSqMeters_is_null()
    {
      var passCountSummaryResult = new PassCountSummaryResult(null, false, 1, 2, 3, 4, 0);
      var result = new CompactionPassCountSummaryResult(passCountSummaryResult);

      Assert.IsNotNull(result);
      Assert.AreEqual(ContractExecutionResult.DefaultMessage, result.Message);
      Assert.IsNull(result.SummaryData);
    }

    [TestMethod]
    public void CreatePassCountSummaryResult_Should_return_full_object_When_totalAreaCoveredSqMeters_is_not_null()
    {
      var targetPassCountRange = new TargetPassCountRange(6, 7);
      var passCountSummaryResult = new PassCountSummaryResult(targetPassCountRange, false, 1, 2, 3, 4, 342.12);
      var result = new CompactionPassCountSummaryResult(passCountSummaryResult);

      Assert.IsNotNull(result);
      Assert.AreEqual(ContractExecutionResult.DefaultMessage, result.Message);

      Assert.AreEqual(342.12, result.SummaryData.TotalAreaCoveredSqMeters);
      Assert.AreEqual(1, result.SummaryData.PercentEqualsTarget);
      Assert.AreEqual(2, result.SummaryData.PercentGreaterThanTarget);
      Assert.AreEqual(3, result.SummaryData.PercentLessThanTarget);
      Assert.IsNotNull(result.SummaryData.PassCountTarget);
      Assert.AreEqual(6, result.SummaryData.PassCountTarget.MinPassCountMachineTarget);
      Assert.AreEqual(7, result.SummaryData.PassCountTarget.MaxPassCountMachineTarget);
    }
  }
}
