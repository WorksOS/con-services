using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.Compaction.ResultHandling
{
  [TestClass]
  public class PassCountDetailedResultTests
  {
    [TestMethod]
    [DataRow(new[] { 0.0, 0.0 }, 0, false)]
    [DataRow(new[] { 1.0, 0.2 }, 0, true)]
    [DataRow(new[] { 0.0, 0.0 }, 0.0009, false)]
    [DataRow(new[] { 1.0, 2.0 }, 0.0009, true)]
    [DataRow(new[] { 0.0, 0.0 }, 0.001, false)]
    [DataRow(new[] { 1.0, 2.0 }, 0.001, true)]
    [DataRow(new[] { 0.0, 0.0 }, 0.1, true)]
    [DataRow(new[] { 0.0, 0.0 }, 10.234, true)]
    public void HasData_Should_return_expected_result_From_coverage_value(double[] percents, double totalAreaCovered, bool expectedResult)
    {
      var obj = new PassCountDetailedResult(null, false, percents, totalAreaCovered);

      Assert.AreEqual(expectedResult, obj.HasData());
    }
  }
}
