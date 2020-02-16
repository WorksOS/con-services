using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VSS.Hosted.VLCommon.Bss
{
  public class DeRegisterDeviceStateOTA: Activity
  {
    public const string SUCCESS_MESSAGE = @"Successfully sent the DeRegistered OTA message for Device: {0}.";
    public const string RETURNED_FALSE_MESSAGE = @"DeRegistered OTA message returned false for an unknown reason for the Device: {0}.";
    public const string EXCEPTION_MESSAGE = @"Failed to send the DeRegistered OTA message to the Device: {0}.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<DeviceStatusContext>();
      bool success;
      try
      {
          success = Services.OTAServices().SendPLOTACommand(Data.Context.OP, context.DeviceAsset.GpsDeviceId);

        if (!success)
          return Error(RETURNED_FALSE_MESSAGE, context.IBKey);
      }
      catch (Exception ex)
      {
        return Exception(ex, EXCEPTION_MESSAGE, context.IBKey);
      }
      return Success(SUCCESS_MESSAGE, context.IBKey);
    }
  }
}
