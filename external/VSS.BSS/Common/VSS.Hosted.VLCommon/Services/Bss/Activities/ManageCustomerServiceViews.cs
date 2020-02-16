using System;
using System.Collections.Generic;

namespace VSS.Hosted.VLCommon.Bss
{
  public class ManageCustomerServiceViews : Activity
  {
    public const string COUNT_IS_ZERO_MESSAGE = @"No ServiceViews updated for Customer.";
    public const string FAILURE_MESSAGE = @"Failed to update ServiceViews for Customer.";
    public const string SUCCESS_MESSAGE = @"{0} ServiceViews udpated for Customer.";
    public const string NULL_RESULT_MESSAGE = @"ServiceView updation came back null for unknown reason.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<DeviceServiceContext>();

      IList<ServiceViewInfoDto> serviceViewsTerminated;

      try
      {
        serviceViewsTerminated = Services.ServiceViews().UpdateServiceAndServiceViews(
          context.ExistingDeviceAsset.AssetId,
          context.ExistingDeviceAsset.OwnerBSSID,
          context.ExistingService.ServiceID,
          context.OwnerVisibilityDate,
          context.ServiceType.Value);

        if (serviceViewsTerminated == null)
          return Error(NULL_RESULT_MESSAGE);
      }
      catch (Exception ex)
      {
        return Exception(ex, FAILURE_MESSAGE);
      }

      if (serviceViewsTerminated.Count == 0)
        return Success(COUNT_IS_ZERO_MESSAGE);

      AddSummary(SUCCESS_MESSAGE, serviceViewsTerminated.Count);
      AddSummary(SummaryHelper.GetServiceViewSummary(serviceViewsTerminated));

      return Success();
    }
  }
}
