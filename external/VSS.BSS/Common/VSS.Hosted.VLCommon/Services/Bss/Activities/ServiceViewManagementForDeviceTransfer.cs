using System;
using System.Collections.Generic;

namespace VSS.Hosted.VLCommon.Bss
{
  public class ServiceViewManagementForDeviceTransfer : Activity
  {
    public const string COUNT_IS_ZERO_MESSAGE = @"No ServiceViews transfered.";
    public const string FAILURE_MESSAGE = @"Failed to transfer ServiceViews.";
    public const string SUCCESS_MESSAGE = @"{0} ServiceViews transfered.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<AssetDeviceContext>();

      Tuple<IList<ServiceViewInfoDto>, IList<ServiceViewInfoDto>> serviceViews = null; 

      try
      {
        //serviceViews = Services.ServiceViews()
        //  .TransferDeviceServiceViews(context.Device.AssetId, context.Asset.AssetId, context.TransferDate);
      }
      catch (Exception ex)
      {
        return Exception(ex, FAILURE_MESSAGE);
      }

      if(serviceViews == null)
        return Success(COUNT_IS_ZERO_MESSAGE);

      if(serviceViews.Item1 == null || serviceViews.Item1.Count == 0)
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
        AddSummary(SummaryHelper.GetServiceViewSummary(serviceViews.Item1));
      }
      
      return Success();
    }
  }
}
