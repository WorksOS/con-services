using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.Compaction.ResultHandling
{
  [TestClass]
  public class CompactionPassCountDetailedResultTests
  {
    [Ignore]
    [TestMethod]
    public void CreatePassCountDetailedResult_Should_return_empty_object_When_TotalCoverageArea_is_null()
    {
      var passCountDetailedResult = PassCountDetailedResult.CreatePassCountDetailedResult(null, false, null, 0);
      var result = CompactionPassCountDetailedResult.CreatePassCountDetailedResult(passCountDetailedResult);

      Assert.IsNotNull(result);
      Assert.AreEqual(ContractExecutionResult.DefaultMessage, result.Message);

      Assert.IsNotNull(result.DetailedData);
      Assert.AreEqual(0, result.DetailedData.TotalCoverageArea);
    }

    [TestMethod]
    public void CreatePassCountDetailedResult_Should_return_full_object_When_TotalCoverageArea_is_not_null()
    {
      var passCountRange = TargetPassCountRange.CreateTargetPassCountRange(45, 67);
      var passCountDetailedResult = PassCountDetailedResult.CreatePassCountDetailedResult(passCountRange, false, new[] { 12.3, 45.6 }, 123.45);

      var result = CompactionPassCountDetailedResult.CreatePassCountDetailedResult(passCountDetailedResult);

      Assert.IsNotNull(result);
      Assert.AreEqual(ContractExecutionResult.DefaultMessage, result.Message);

      Assert.IsNotNull(result.DetailedData.PassCountTarget);
      Assert.IsNotNull(result.DetailedData.Percents);
      Assert.IsNotNull(result.DetailedData.TotalCoverageArea);
    }
  }
}