using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSS.Hosted.VLCommon.Bss
{
  public class ActivatedServicePlanConfiguration : Activity
  {
    public const string SUCCESS_MESSAGE = @"ServicePlans configured successfuly for Asset: {0}.";
    public const string RETURNED_FALSE_MESSAGE = @"ServicePlans configuration returned false for an unknown reason.";
    public const string EXCEPTION_MESSAGE = @"Failed to configure ServicePlans for the Asset: {0}.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<DeviceServiceContext>();
      bool success;
      try
      {
        success = Services.ServiceViews().ConfigureDeviceForActivatedServicePlans(
          context.ExistingDeviceAsset.AssetId,
          context.ActionUTC,
          context.ExistingDeviceAsset.GpsDeviceId,
          context.ExistingDeviceAsset.Type.Value,
          context.ServiceType.Value);

        if (!success)
          return Error(RETURNED_FALSE_MESSAGE);
      }
      catch (Exception ex)
      {
        return Exception(ex, EXCEPTION_MESSAGE, context.ExistingDeviceAsset.AssetId);
      }
      return Success(SUCCESS_MESSAGE, context.ExistingDeviceAsset.AssetId);
    }
  }
}
