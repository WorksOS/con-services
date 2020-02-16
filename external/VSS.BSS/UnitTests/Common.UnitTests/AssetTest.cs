using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon;

namespace UnitTests
{
  /// <summary>
  ///This is a test class for AssetTest and is intended
  ///to contain all AssetTest Unit Tests
  ///</summary>
  [TestClass()]
  public class AssetTest
  {
    /// <summary>
    ///A test for ComputeAssetID
    ///</summary>
    [TestMethod()]
    public void ComputeAssetIDTest()
    {
      string makeCode = "TTT";  // Trimble Test Tractor
      string serialNumberVIN = "VL0013456SN";
      long actualAssetID = Asset.ComputeAssetID(makeCode, serialNumberVIN);
      Assert.AreEqual(4125161519618216, actualAssetID, "The assetID hashing algorithm appears to have been modified. Oh dear.");
    }
  }
}
