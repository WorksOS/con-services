using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectWebApiCommon.ResultsHandling;

namespace MasterDataConsumerTests
{
  [TestClass]
  public class ContractExecutionStatesEnumTests
  {
    [TestMethod]
    public void DynamicAddwithOffsetTest()
    {
      var contractExecutionStatesEnum = new ContractExecutionStatesEnum();
      Assert.AreEqual(68, contractExecutionStatesEnum.DynamicCount);
      Assert.AreEqual("Supplied CoordinateSystem filename is not valid. Exceeds the length limit of 256, is empty, or contains illegal characters.", contractExecutionStatesEnum.FirstNameWithOffset(2));
      Assert.AreEqual("LegacyImportedFileId has not been generated.", contractExecutionStatesEnum.FirstNameWithOffset(50));
    }
  }
}
