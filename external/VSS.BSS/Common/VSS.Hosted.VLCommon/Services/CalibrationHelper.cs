using System;
using System.Linq;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon
{
  public static class CalibrationHelper
  {
      public static bool CalibrateDeviceRuntime(INH_OP opCtx1, string[] gpsDeviceIDs, DeviceTypeEnum deviceType, double newRuntimeHours)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0 || deviceType == DeviceTypeEnum.Series522 || deviceType == DeviceTypeEnum.Series523 || API.Device.IsProductLinkDevice(deviceType))
        throw new ArgumentException("Invalid parameters");

      bool success = true;
      int result = -1;
      //Special case to mark as sent directly after the button is pressed because it is not really sent to the device
      RuntimeAdjConfig calibrationConfig = new RuntimeAdjConfig();
      calibrationConfig.Runtime = TimeSpan.FromHours(newRuntimeHours);
      calibrationConfig.Status = MessageStatusEnum.Sent;
      calibrationConfig.SentUTC = DateTime.UtcNow;
      calibrationConfig.MessageSourceID = -1;

      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        int deviceTypeID = (int)deviceType;
        RuntimeCalibration calibration;
            calibration = (from c in opCtx1.RuntimeCalibration
                              where c.SerialNumber == gpsDeviceID
                              && c.CurrentDeviceSMH == null
                              && c.DeviceType == deviceTypeID
                              select c).FirstOrDefault();
          if (calibration == null)
          {
            calibration = new RuntimeCalibration
            {
              SerialNumber = gpsDeviceID,
              DeviceType = (int)deviceType,
              NewCalibrationHours = newRuntimeHours,
              InsertUTC = DateTime.UtcNow,
              UpdateUTC = DateTime.UtcNow
            };
            opCtx1.RuntimeCalibration.AddObject(calibration);
          }
          else
          {
            calibration.NewCalibrationHours = newRuntimeHours;
            calibration.UpdateUTC = DateTime.UtcNow;
          }

          result = opCtx1.SaveChanges();

        if (result <= 0)
          break;

        calibrationConfig.MessageSourceID = calibration.ID;

        ConfigStatusSvcClient.ProcessDeviceConfiguration(gpsDeviceID, deviceType, calibrationConfig);
      }

      if (result <= 0)
        throw new InvalidOperationException("Failed to save Runtime Calibration");

      return success;
    }
  }
}
