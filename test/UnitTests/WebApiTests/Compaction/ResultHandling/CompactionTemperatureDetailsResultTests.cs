using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.Compaction.ResultHandling
{
  [TestClass]
  public class CompactionTemperatureDetailsResultTests
  {
    [TestMethod]
    public void CreateTemperatureDetailResult_Should_return_null_object_When_TotalAreaCoveredSqMeters_is_null()
    {
      var result = new CompactionTemperatureDetailResult(new double[]{},0);

      Assert.IsNotNull(result);
      Assert.AreEqual(ContractExecutionResult.DefaultMessage, result.Message);
      Assert.IsNull(result.DetailsData);
    }

    [TestMethod]
    public void CreateTemperatureDetailResult_Should_return_full_object_When_TotalAreaCoveredSqMeters_is_not_null()
    {
      var targets = new[] { 3.4, 41.89, 11.3, 29.4, 15.2 };
      var area = 6923.5;
      var result = new CompactionTemperatureDetailResult(targets, area);

      Assert.IsNotNull(result);
      Assert.AreEqual(ContractExecutionResult.DefaultMessage, result.Message);
      Assert.IsNotNull(result.DetailsData);
      Assert.AreEqual(area, result.DetailsData.TotalAreaCoveredSqMeters);
      Assert.AreEqual(targets, result.DetailsData.Percents);
    }
  }
}
