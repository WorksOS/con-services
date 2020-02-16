using System;
using System.Collections.Generic;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DevicePersonalityCreate : Activity
  {
    public const string COUNT_IS_ZERO_MESSAGE = @"No DevicePersonalities created for IBKey: {0}.";
    public const string DEVICE_PERSONALITY_INFO_TEMPLATE = @"PersonalityType: {0} Value: {1}.";
    public const string FAILURE_MESSAGE = @"Failed to create DevicePersonalities for IBKey: {0}.";
    public const string SUCCESS_MESSAGE = @"{0} DevicePersonalities created.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<AssetDeviceContext>();

      Require.IsNotNull(context.IBDevice, "DeviceContext.IBDevice");

      IList<DevicePersonality> devicePersonalities = null;

      try
      {
        devicePersonalities = Services.Devices().CreateDevicePersonality(context);

        if (devicePersonalities == null || devicePersonalities.Count == 0)
          return Warning(COUNT_IS_ZERO_MESSAGE, context.IBDevice.IbKey);
      }
      catch (Exception ex)
      {
        return Exception(ex, FAILURE_MESSAGE, context.IBDevice.IbKey);
      }

      return Success(GetSuccessSummary(devicePersonalities).ToNewLineString());
    }

    #region Private Methods

    private List<string> GetSuccessSummary(IList<DevicePersonality> devicePersonalities)
    {
      var summary = new List<string> { string.Format(SUCCESS_MESSAGE, devicePersonalities.Count) };

      foreach (var devicePersonality in devicePersonalities)
      {
        summary.Add(string.Format(DEVICE_PERSONALITY_INFO_TEMPLATE,
          (PersonalityTypeEnum)devicePersonality.fk_PersonalityTypeID,
          devicePersonality.Value));
      }

      return summary;
    }
    #endregion
  }
}
