using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;

namespace WebApiTests.Models
{
  [TestClass]
  public class ContractExecutionStatesEnumTests
  {
    [TestMethod]
    public void DynamicAddwithOffsetTest()
    {
      var contractExecutionStatesEnum = new ContractExecutionStatesEnum();
      Assert.AreEqual(50, contractExecutionStatesEnum.DynamicCount);
      Assert.AreEqual("AssetId, if present, must be >= -1", contractExecutionStatesEnum.FirstNameWithOffset(2));
      Assert.AreEqual("DeviceType is invalid", contractExecutionStatesEnum.FirstNameWithOffset(30));
    }
  }
}
