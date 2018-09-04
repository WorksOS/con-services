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
  public class CompactionPassCountDetailedResultTests
  {
    [TestMethod]
    public void CreatePassCountDetailedResult_Should_return_null_object_When_TotalCoverageArea_is_null()
    {
      var passCountDetailedResult = new PassCountDetailedResult(null, false, null, 0);
      var result = new CompactionPassCountDetailedResult(passCountDetailedResult);

      Assert.IsNotNull(result);
      Assert.AreEqual(ContractExecutionResult.DefaultMessage, result.Message);
      Assert.IsNull(result.DetailedData);
    }

    [TestMethod]
    public void CreatePassCountDetailedResult_Should_return_full_object_When_TotalCoverageArea_is_not_null()
    {
      var passCountRange = new TargetPassCountRange(45, 67);
      var passCountDetailedResult = new PassCountDetailedResult(passCountRange, false, new[] { 12.3, 45.6 }, 123.45);

      var result = new CompactionPassCountDetailedResult(passCountDetailedResult);

      Assert.IsNotNull(result);
      Assert.AreEqual(ContractExecutionResult.DefaultMessage, result.Message);

      Assert.IsNotNull(result.DetailedData.PassCountTarget);
      Assert.IsNotNull(result.DetailedData.Percents);
      Assert.IsNotNull(result.DetailedData.TotalCoverageArea);
    }
  }
}
