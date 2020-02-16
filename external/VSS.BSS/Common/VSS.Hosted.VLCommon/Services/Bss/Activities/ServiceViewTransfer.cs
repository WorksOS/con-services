using System;
using System.Collections.Generic;

namespace VSS.Hosted.VLCommon.Bss
{
  public class ServiceViewTransfer : Activity
  {
    public const string FAILURE_MESSAGE = @"Failed to swap Service Views from Old Asset: {0} to New Asset: {1}.";
    public const string COUNT_IS_ZERO_MESSAGE = @"No ServiceViews swapped.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<DeviceAssetContext>();

      Tuple<IList<ServiceViewInfoDto>, IList<ServiceViewInfoDto>> serviceViews;

      try
      {
        serviceViews = Services.ServiceViews().SwapServiceViewsBetweenOldAndNewAsset(
          context.OldDeviceAsset.AssetId,
          context.NewDeviceAsset.AssetId,
          context.ActionUTC);
      }
      catch (Exception ex)
      {
        return Exception(ex, FAILURE_MESSAGE, context.OldDeviceAsset.AssetId, context.NewDeviceAsset.AssetId);
      }

      if (serviceViews == null)
        return Success(COUNT_IS_ZERO_MESSAGE);

      if (serviceViews.Item1 == null || serviceViews.Item1.Count == 0)
        AddSummary("No ServiceViews were terminated.");
      else
      {
        AddSummary("{0} ServiceViews terminated.", serviceViews.Item1.Count);
        AddSummary(SummaryHelper.GetServiceViewSummary(serviceViews.Item1));
      }

      if (serviceViews.Item2 == null || serviceViews.Item2.Count == 0)
        AddSummary("No ServiceViews were created.");
      else
      {
        AddSummary("{0} ServiceViews created.", serviceViews.Item2.Count);
        AddSummary(SummaryHelper.GetServiceViewSummary(serviceViews.Item2));
      }
      return Success();
    }

  }
}
