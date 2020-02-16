using System;
using System.Collections.Generic;

namespace VSS.Hosted.VLCommon.Bss
{
  public class ServiceViewManagementForOwnershipTransfer : Activity
  {
    public const string COUNT_IS_ZERO_MESSAGE = @"No ServiceViews transfered.";
    public const string FAILURE_MESSAGE = @"Failed to transfer ServiceViews.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<AssetDeviceContext>();

      List<ServiceViewInfoDto> terminatedViews = new List<ServiceViewInfoDto>();
      List<ServiceViewInfoDto> createdViews = new List<ServiceViewInfoDto>();

      try
      {
        terminatedViews.AddRange(Services.ServiceViews().TerminateServiceViewsForAsset(context.Asset.AssetId, context.TransferDate));
        createdViews.AddRange(Services.ServiceViews().CreateServiceViewsForAsset(context.Asset.AssetId, context.TransferDate));
      }
      catch (Exception ex)
      {
        return Exception(ex, FAILURE_MESSAGE);
      }

      if (terminatedViews.Count == 0 || createdViews.Count == 0)
        return Success(COUNT_IS_ZERO_MESSAGE);

      if (terminatedViews.Count == 0)
        AddSummary("No ServiceViews were terminated.");
      else
      {
        AddSummary("{0} ServiceViews terminated.", terminatedViews.Count);
        AddSummary(SummaryHelper.GetServiceViewSummary(terminatedViews));
      }

      if (createdViews.Count == 0)
        AddSummary("No ServiceViews were created.");
      else
      {
        AddSummary("{0} ServiceViews created.", createdViews.Count);
        AddSummary(SummaryHelper.GetServiceViewSummary(createdViews));
      }

      return Success();
    }
  }
}
