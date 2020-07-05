using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.TagFileAuth.Models;

namespace WebApiTests.Models
{
  [TestClass]
  public class ContractExecutionStatesEnumTests
  {
    [TestMethod]
    public void DynamicAddwithOffsetTest()
    {
      var contractExecutionStatesEnum = new ContractExecutionStatesEnum();
      Assert.AreEqual(23, contractExecutionStatesEnum.DynamicCount);
      Assert.AreEqual("Manual Import: Unable to determine lat/long from northing/easting position", contractExecutionStatesEnum.FirstNameWithOffset(18));
      Assert.AreEqual("Unable to locate projects for device in cws", contractExecutionStatesEnum.FirstNameWithOffset(105));
    }
  }
}
