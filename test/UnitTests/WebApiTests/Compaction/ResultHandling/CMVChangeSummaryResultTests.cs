using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.Compaction.ResultHandling
{
  [TestClass]
  public class CMVChangeSummaryResultTests
  {
    [TestMethod]
    [DataRow(0, false)]
    [DataRow(0.0009, false)]
    [DataRow(0.001, false)]
    [DataRow(0.1, true)]
    [DataRow(10.234, true)]
    public void HasData_Should_return_expected_result_From_coverage_value(double coverageArea, bool expectedResult)
    {
      var obj = CMVChangeSummaryResult.Create(null, coverageArea);

      Assert.AreEqual(expectedResult, obj.HasData());
    }
  }
}