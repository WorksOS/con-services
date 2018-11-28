using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;

namespace VSS.MasterData.ProjectTests
{
  [TestClass]
  public class ContractExecutionStatesEnumTests
  {
    [TestMethod]
    public void DynamicAddwithOffsetTest()
    {
      var projectErrorCodesProvider = new ProjectErrorCodesProvider();
      Assert.AreEqual(114, projectErrorCodesProvider.DynamicCount);
      Assert.AreEqual("Supplied CoordinateSystem filename is not valid. Exceeds the length limit of 256, is empty, or contains illegal characters.", projectErrorCodesProvider.FirstNameWithOffset(2));
      Assert.AreEqual("LegacyImportedFileId has not been generated.", projectErrorCodesProvider.FirstNameWithOffset(50));
    }
  }
}