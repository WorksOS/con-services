using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VSS.Hydrology.Tests.Ponding.ResultHandling
{
  [TestClass]
  public class PondingResultTests
  {
    [TestMethod]
    [DataRow(0, false)]
    [DataRow(0.0009, false)]
    [DataRow(0.001, false)]
    [DataRow(0.1, true)]
    [DataRow(10.234, true)]
    public void PondingResultTest(double totalAreaCovered, bool expectedResult)
    {
      //var obj = new MDPSummaryResult(0, 0, false, 0, 0, totalAreaCovered, 0);

      //Assert.AreEqual(expectedResult, obj.HasData());
      Assert.Fail();
    }
  }
}
