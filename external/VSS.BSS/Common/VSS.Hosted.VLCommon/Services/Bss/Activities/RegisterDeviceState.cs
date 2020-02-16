using System;

namespace VSS.Hosted.VLCommon.Bss
{
  public class RegisterDeviceState : Activity
  {
    public const string SUCCESS_MESSAGE = @"Device: {0} State successfuly Registered.";
    public const string EXCEPTION_MESSAGE = @"Failed to Register Device: {0} state.";
    
    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<DeviceStatusContext>();

      try
      {
        Services.Devices().RegisterDevice(context.DeviceAsset.DeviceId);
      }
      catch (Exception ex)
      {
        return Exception(ex, EXCEPTION_MESSAGE, context.IBKey);
      }
      return Success(SUCCESS_MESSAGE, context.IBKey);
    }
  }
}
