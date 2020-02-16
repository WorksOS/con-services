using System;
using VSS.UnitTest.Common.Contexts;


using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common
{
  public class NHRawHelper
  {
    public void AddDeviceToRawDevice(Device device)
    {
      switch ((DeviceTypeEnum)device.fk_DeviceTypeID)
      {
        case DeviceTypeEnum.PL121:
        case DeviceTypeEnum.PL321:
          var plDevice = new PLDevice
                         {
                           ModuleCode = device.GpsDeviceID,
                           InAmericas = true,
                           IsReadOnly = true,
                           UpdateUTC = DateTime.UtcNow
                         };
          ContextContainer.Current.RawContext.PLDevice.AddObject(plDevice);
          break;
        case DeviceTypeEnum.Series521:
        case DeviceTypeEnum.Series522:
        case DeviceTypeEnum.Series523:
        case DeviceTypeEnum.SNM940:
        case DeviceTypeEnum.CrossCheck:
        case DeviceTypeEnum.PL420:
        case DeviceTypeEnum.PL421:
        case DeviceTypeEnum.SNM451:
          var mtsDevice = new MTSDevice
                          {
                            SerialNumber = device.GpsDeviceID,
                            DeviceType = device.fk_DeviceTypeID,
                            IsTCP = true,
                            SampleRate = (int) ServiceType.DefaultSamplingInterval.TotalSeconds,
                            UpdateRate = (int) ServiceType.DefaultReportingInterval.TotalSeconds,
                            LowPowerRate = (int) ServiceType.DefaultLowPowerInterval.TotalSeconds,
                            BitPacketRate = (int) ServiceType.DefaultBitPacketInterval.TotalSeconds,
                            IpAddress = "0.0.0.0",
                            UpdateUTC = DateTime.UtcNow
                          };
          ContextContainer.Current.RawContext.MTSDevice.AddObject(mtsDevice);
          break;
        case DeviceTypeEnum.TrimTrac:
          var ttDevice = new TTDevice
                         {
                           IMEI = device.GpsDeviceID,
                           UnitID = API.Device.IMEI2UnitID(device.GpsDeviceID),
                           UpdateUTC = device.UpdateUTC
                         };
          ContextContainer.Current.RawContext.TTDevice.AddObject(ttDevice);
          break;
        default:
          break;
      }

      ContextContainer.Current.RawContext.SaveChanges();
    }
  }
}