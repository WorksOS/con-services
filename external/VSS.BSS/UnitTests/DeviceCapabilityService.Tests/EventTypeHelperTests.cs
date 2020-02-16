using Microsoft.VisualStudio.TestTools.UnitTesting;
//using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ExternalDataTypes.Enumerations;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.Helpers;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.Query;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests
{
  [TestClass]
  public class EventTypeHelperTests
  {
    [TestMethod]
    public void TestGetQueryString()
    {
      Assert.AreEqual("assetid=1111111", EventTypeHelper.GetQueryString(new DeviceQuery { AssetID = 1111111 }));
      Assert.AreEqual("id=123", EventTypeHelper.GetQueryString(new DeviceQuery { ID = 123 }));
      Assert.AreEqual("gpsdeviceid=ple641Device&devicetype=PLE641", EventTypeHelper.GetQueryString(new DeviceQuery { GPSDeviceID = "ple641Device", DeviceType = DeviceTypeEnum.PLE641 }));
      Assert.AreEqual(string.Empty, EventTypeHelper.GetQueryString(new DeviceQuery()));
    }
  }
}
