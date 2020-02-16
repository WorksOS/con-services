using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class ServicePlanActivate : Activity
  {
    public const string FAILURE_MESSAGE = @"Failed to create Service Views for Device: {0} and Service: {1}.";
    public const string COUNT_IS_ZERO_MESSAGE = @"No Service and Service Views Created.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<DeviceServiceContext>();

      Tuple<Service, IList<ServiceViewInfoDto>> serviceServiceViews;

      try
      {
        serviceServiceViews = Services.ServiceViews().CreateServiceAndServiceViews(
          context.ExistingDeviceAsset.DeviceId, 
          context.ExistingDeviceAsset.Type.Value, 
          context.PlanLineID, 
          context.ActionUTC, 
          context.OwnerVisibilityDate, 
          context.ServiceType.Value);

        if (serviceServiceViews != null && serviceServiceViews.Item1 != null)
          context.MapServiceToExistingService(serviceServiceViews.Item1);
      }
      catch (Exception ex)
      {
        return Exception(ex, FAILURE_MESSAGE, context.IBKey, context.ServiceType);
      }

      if (serviceServiceViews == null || serviceServiceViews.Item1 == null)
        return Error(COUNT_IS_ZERO_MESSAGE);

      if(serviceServiceViews.Item2 == null || serviceServiceViews.Item2.Count == 0)
        AddSummary("No ServiceViews were created.");
      else
      {
        AddSummary("{0} ServiceViews created.", serviceServiceViews.Item2.Count);
        AddSummary(SummaryHelper.GetServiceViewSummary(serviceServiceViews.Item2));
      }

      return Success();
    }
  }
}
