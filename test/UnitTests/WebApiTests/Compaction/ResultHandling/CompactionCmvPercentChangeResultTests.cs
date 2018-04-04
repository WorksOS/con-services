using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.Compaction.ResultHandling
{
  [TestClass]
  public class CompactionCmvPercentChangeResultTests
  {
    [TestMethod]
    public void CreateCmvPercentChangeResult_Should_return_null_object_When_CoverageArea_is_null()
    {
      var changeSummaryResult = CMVChangeSummaryResult.Create(null, 0);
      var result = CompactionCmvPercentChangeResult.CreateCmvPercentChangeResult(changeSummaryResult);

      Assert.IsNotNull(result);
      Assert.AreEqual(ContractExecutionResult.DefaultMessage, result.Message);
      Assert.IsNull(result.SummaryData);
    }

    [TestMethod]
    public void CreateCmvPercentChangeResult_Should_return_full_object_When_CoverageArea_is_not_null()
    {
      var changeSummaryResult = CMVChangeSummaryResult.Create(new[] { 3.4, 5.6 }, 34534);
      var result = CompactionCmvPercentChangeResult.CreateCmvPercentChangeResult(changeSummaryResult);

      Assert.IsNotNull(result);
      Assert.AreEqual(ContractExecutionResult.DefaultMessage, result.Message);

      Assert.AreEqual(34534, result.SummaryData.TotalAreaCoveredSqMeters);
      Assert.IsNotNull(result.SummaryData.Percents);
    }
  }
}