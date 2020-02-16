using System;
using System.Collections.Generic;

namespace VSS.Hosted.VLCommon.Bss
{
  public class ServiceViewCreateForDeviceReplacement : Activity
  {
    public const string COUNT_IS_ZERO_MESSAGE = @"No ServiceViews created.";
    public const string FAILURE_MESSAGE = @"Failed to create ServiceViews.";
    public const string SUCCESS_MESSAGE = @"{0} ServiceViews created.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<DeviceAssetContext>();

      IList<ServiceViewInfoDto> createdViews;

      try
      {
        createdViews = Services.ServiceViews().CreateServiceViewsForAsset(context.NewDeviceAsset.AssetId, context.ActionUTC);
      }
      catch (Exception ex)
      {
        return Exception(ex, FAILURE_MESSAGE);
      }

      if (createdViews == null || createdViews.Count == 0)
        return Success(COUNT_IS_ZERO_MESSAGE);

      AddSummary(SUCCESS_MESSAGE, createdViews.Count);
      return Success(SummaryHelper.GetServiceViewSummary(createdViews));
    }
  }
}