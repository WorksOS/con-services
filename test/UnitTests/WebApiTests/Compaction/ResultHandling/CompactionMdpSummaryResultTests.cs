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
  public class CompactionMdpSummaryResultTests
  {
    [TestMethod]
    public void CreateMdpSummaryResult_Should_return_null_object_When_TotalAreaCoveredSqMeters_is_null()
    {
      var mdpSummaryResult = MDPSummaryResult.Create(1, 2, true, 3, 4, 0, 5);
      var result = CompactionMdpSummaryResult.CreateMdpSummaryResult(mdpSummaryResult, null);

      Assert.IsNotNull(result);
      Assert.AreEqual(ContractExecutionResult.DefaultMessage, result.Message);
      Assert.IsNull(result.SummaryData);
    }

    [TestMethod]
    public void CreateMdpSummaryResult_Should_return_full_object_When_TotalAreaCoveredSqMeters_is_not_null()
    {
      var mdpSummaryResult = MDPSummaryResult.Create(1, 2, true, 3, 4, 3425, 5);
      var mdpSettings = MDPSettings.CreateMDPSettings(7, 8, 9, 10, 11, true);

      var result = CompactionMdpSummaryResult.CreateMdpSummaryResult(mdpSummaryResult, mdpSettings);

      Assert.IsNotNull(result);
      Assert.AreEqual(ContractExecutionResult.DefaultMessage, result.Message);

      Assert.AreEqual(9, result.SummaryData.MaxMDPPercent);
      Assert.AreEqual(11, result.SummaryData.MinMDPPercent);
      Assert.AreEqual(5, result.SummaryData.PercentLessThanTarget);
      Assert.AreEqual(3, result.SummaryData.PercentGreaterThanTarget);
      Assert.AreEqual(3425, result.SummaryData.TotalAreaCoveredSqMeters);
      Assert.IsNotNull(result.SummaryData.MdpTarget);
    }
  }
}
