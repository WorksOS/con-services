using System;
using System.Collections.Generic;

namespace VSS.Hosted.VLCommon.Bss
{
  public class ServiceViewTerminateForOldAsset : Activity
  {
    public const string COUNT_IS_ZERO_MESSAGE = @"No ServiceViews terminated.";
    public const string FAILURE_MESSAGE = @"Failed to terminated ServiceViews.";
    public const string SUCCESS_MESSAGE = @"{0} ServiceViews terminated.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<AssetDeviceContext>();

      IList<ServiceViewInfoDto> terminatedViews;

      try
      {
        terminatedViews = Services.ServiceViews().TerminateServiceViewsForAsset(context.Device.AssetId, context.TransferDate);
      }
      catch (Exception ex)
      {
        return Exception(ex, FAILURE_MESSAGE);
      }

      if (terminatedViews == null || terminatedViews.Count == 0)
        return Success(COUNT_IS_ZERO_MESSAGE);

      AddSummary(SUCCESS_MESSAGE, terminatedViews.Count);
      return Success(SummaryHelper.GetServiceViewSummary(terminatedViews));
    }
  }
}