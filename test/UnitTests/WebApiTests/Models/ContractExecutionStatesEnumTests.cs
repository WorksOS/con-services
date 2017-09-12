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
      Assert.AreEqual(32, contractExecutionStatesEnum.DynamicCount);
      Assert.AreEqual("AssetId, if present, must be >= -1", contractExecutionStatesEnum.FirstNameWithOffset(2));
      Assert.AreEqual("Unable to identify a customerUid", contractExecutionStatesEnum.FirstNameWithOffset(29));
    }
  }
}
