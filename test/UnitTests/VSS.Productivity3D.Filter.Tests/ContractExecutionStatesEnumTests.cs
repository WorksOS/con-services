using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Filter.Common.ResultHandling;

namespace VSS.Productivity3D.Filter.Tests
{
  [TestClass]
  public class ContractExecutionStatesEnumTests
  {
    [TestMethod]
    public void DynamicAddwithOffsetTest()
    {
      var contractExecutionStatesEnum = new ContractExecutionStatesEnum();
      Assert.AreEqual(28, contractExecutionStatesEnum.DynamicCount);
      Assert.AreEqual("Invalid filterUid.", contractExecutionStatesEnum.FirstNameWithOffset(2));
      Assert.AreEqual("UpsertFilter failed. Unable to create persistant filter.",
        contractExecutionStatesEnum.FirstNameWithOffset(24));
    }
  }
}