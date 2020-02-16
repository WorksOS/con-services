using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class BssUnitTestBase : UnitTestBase
  {
    private BssTestHelper _testHelper = new BssTestHelper();
    public BssTestHelper TestHelper
    {
      get { return _testHelper; }
      set { _testHelper = value; }
    }

    [TestInitialize]
    public void BssUnitTestBase_Init()
    {
      Services.Customers = () => new BssCustomerService();
      Services.ServiceViews = () => new BssServiceViewService();
      Services.Devices = () => new BssDeviceService();
      Services.Assets = () => new BssAssetService();
      Services.AssetDeviceHistory = () => new BssAssetDeviceHistoryService();
      Services.OTAServices = () => new BssPLOTAService();
    }

    [TestCleanup]
    public void BssUnitTestBase_Cleanup()
    {
      Data.Context.Dispose();
    }
  }
}
