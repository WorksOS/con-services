using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.Compaction.ResultHandling
{
  [TestClass]
  public class CompactionCmvSummaryResultTests
  {
    [TestMethod]
    public void CreateCmvSummaryResult_Should_return_null_object_When_TotalAreaCoveredSqMeters_is_null()
    {
      var summaryResult = CMVSummaryResult.Create(1, 2, true, 3, 4, 0, 6);
      var settings = CMVSettings.CreateCMVSettings(1, 2, 3, 4, 5, true);

      var result = CompactionCmvSummaryResult.Create(summaryResult, settings);

      Assert.IsNotNull(result);
      Assert.IsNull(result.SummaryData);
      Assert.AreEqual(ContractExecutionResult.DefaultMessage, result.Message);
    }

    [TestMethod]
    public void CreateCmvSummaryResult_Should_return_full_object_When_TotalAreaCoveredSqMeters_is_not_null()
    {
      var summaryResult = CMVSummaryResult.Create(1, 2, true, 3, 4, 5, 6);
      var settings = CMVSettings.CreateCMVSettings(1, 2, 3, 4, 5, true);

      var result = CompactionCmvSummaryResult.Create(summaryResult, settings);

      Assert.IsNotNull(result);
      Assert.AreEqual(ContractExecutionResult.DefaultMessage, result.Message);

      Assert.AreEqual(5, result.SummaryData.TotalAreaCoveredSqMeters);
      Assert.AreEqual(3, result.SummaryData.MaxCMVPercent);
      Assert.AreEqual(5, result.SummaryData.MinCMVPercent);
      Assert.AreEqual(1, result.SummaryData.PercentEqualsTarget);
      Assert.AreEqual(3, result.SummaryData.PercentGreaterThanTarget);
      Assert.AreEqual(6, result.SummaryData.PercentLessThanTarget);
      Assert.IsNotNull(result.SummaryData.CmvTarget);
    }
  }
}