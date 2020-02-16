using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
//using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ExternalDataTypes.Enumerations;
using VSS.Nighthawk.DeviceCapabilityService.Helpers;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.Query;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests
{
  [TestClass]
  public class DeviceQueryHelperTests
  {
    [TestMethod]
    public void TestAssetID()
    {
      IDeviceQuery deviceQuery = new DeviceQuery
      {
        AssetID = 1111111
      };
      var mockStorage = new Mock<IStorage>();
      mockStorage.Setup(o => o.GetDeviceTypeForAsset(It.IsAny<long>())).Returns(DeviceTypeEnum.Series523);
      mockStorage.Setup(o => o.GetDeviceTypeForDevice(It.IsAny<long>())).Returns(DeviceTypeEnum.TAP66);
      IDeviceQueryHelper deviceQueryHelper = new DeviceQueryHelper();
      Assert.AreEqual(DeviceTypeEnum.Series523, deviceQueryHelper.GetDeviceType(deviceQuery, mockStorage.Object));
      Assert.AreEqual("AssetID=1111111", deviceQueryHelper.GetPrintableValues(deviceQuery));
    }

    [TestMethod]
    public void TestDeviceID()
    {
      IDeviceQuery deviceQuery = new DeviceQuery
      {
        ID = 123
      };
      var mockStorage = new Mock<IStorage>();
      mockStorage.Setup(o => o.GetDeviceTypeForAsset(It.IsAny<long>())).Returns(DeviceTypeEnum.Series523);
      mockStorage.Setup(o => o.GetDeviceTypeForDevice(It.IsAny<long>())).Returns(DeviceTypeEnum.TAP66);
      IDeviceQueryHelper deviceQueryHelper = new DeviceQueryHelper();
      Assert.AreEqual(DeviceTypeEnum.TAP66, deviceQueryHelper.GetDeviceType(deviceQuery, mockStorage.Object));
      Assert.AreEqual("ID=123", deviceQueryHelper.GetPrintableValues(deviceQuery));
    }

    [TestMethod]
    public void TestDeviceType()
    {
      IDeviceQuery deviceQuery = new DeviceQuery
      {
        DeviceType = DeviceTypeEnum.PLE641,
        GPSDeviceID = "ple641Device"
      };
      var mockStorage = new Mock<IStorage>();
      mockStorage.Setup(o => o.GetDeviceTypeForAsset(It.IsAny<long>())).Returns(DeviceTypeEnum.Series523);
      mockStorage.Setup(o => o.GetDeviceTypeForDevice(It.IsAny<long>())).Returns(DeviceTypeEnum.TAP66);
      IDeviceQueryHelper deviceQueryHelper = new DeviceQueryHelper();
      Assert.AreEqual(DeviceTypeEnum.PLE641, deviceQueryHelper.GetDeviceType(deviceQuery, mockStorage.Object));
      Assert.AreEqual("GPSDeviceID=ple641Device DeviceType=PLE641", deviceQueryHelper.GetPrintableValues(deviceQuery));
    }

    [TestMethod]
    public void TestNothingInQuery()
    {
      IDeviceQuery deviceQuery = new DeviceQuery(); // nothing in the query
      var mockStorage = new Mock<IStorage>();
      mockStorage.Setup(o => o.GetDeviceTypeForAsset(It.IsAny<long>())).Returns(DeviceTypeEnum.Series523);
      mockStorage.Setup(o => o.GetDeviceTypeForDevice(It.IsAny<long>())).Returns(DeviceTypeEnum.TAP66);
      IDeviceQueryHelper deviceQueryHelper = new DeviceQueryHelper();
      Assert.AreEqual(DeviceTypeEnum.MANUALDEVICE, deviceQueryHelper.GetDeviceType(deviceQuery, mockStorage.Object));
      Assert.AreEqual(string.Empty, deviceQueryHelper.GetPrintableValues(deviceQuery));
    }
  }
}
