using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DeregisterDeviceState : Activity
  {
    public const string SUCCESS_MESSAGE = @"Device: {0} successfuly Deregistered.";
    public const string EXCEPTION_MESSAGE = @"Failed to Deregister the Device: {0}.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<DeviceStatusContext>();
      try
      {
        var deviceState = DeviceStateEnum.DeregisteredStore;
        if (context.Status.IsStringEqual("DEREG_TECH"))
          deviceState = DeviceStateEnum.DeregisteredTechnician;

        Services.Devices().DeregisterDeviceState(context.DeviceAsset.DeviceId, deviceState, context.ActionUTC);
      }
      catch (Exception ex)
      {
        return Exception(ex, EXCEPTION_MESSAGE, context.IBKey);
      }
      return Success(SUCCESS_MESSAGE, context.IBKey);
    }
  }
}
