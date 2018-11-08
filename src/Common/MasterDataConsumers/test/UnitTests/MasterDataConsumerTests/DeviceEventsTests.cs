using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.MasterDataConsumer.Tests
{
  [TestClass]
  public class DeviceEventsTests
  {
    [TestMethod]
    public void DeviceEventsCopyModels()
    {
      DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);

      var Device = new Device()
      {
        DeviceUID = Guid.NewGuid().ToString(),
        DeviceSerialNumber = "The Radio SerialNumber",
        DeviceType = "whatever",
        LastActionedUtc = now
      };

      var kafkaDeviceEvent = CopyModel(Device);
      var copiedDevice = CopyModel(kafkaDeviceEvent);

      Assert.AreEqual(Device, copiedDevice, "Device model conversion not completed sucessfully");
    }

    #region private
    private CreateDeviceEvent CopyModel(Device Device)
    {
      return new CreateDeviceEvent()
      {
        DeviceUID = Guid.Parse(Device.DeviceUID),
        DeviceSerialNumber = Device.DeviceSerialNumber,
        DeviceType = Device.DeviceType,
        ActionUTC = Device.LastActionedUtc.HasValue ? Device.LastActionedUtc.Value : new DateTime(2017, 1, 1)
    };
    }

    private Device CopyModel(CreateDeviceEvent kafkaDeviceEvent)
    {
      return new Device()
      {
        DeviceUID = kafkaDeviceEvent.DeviceUID.ToString(),
        DeviceSerialNumber = kafkaDeviceEvent.DeviceSerialNumber,
        DeviceType = kafkaDeviceEvent.DeviceType,
        LastActionedUtc = kafkaDeviceEvent.ActionUTC
      };
    }
    #endregion

  }
}
