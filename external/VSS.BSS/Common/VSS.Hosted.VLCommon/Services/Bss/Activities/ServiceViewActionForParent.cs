using System;
using System.Collections.Generic;

namespace VSS.Hosted.VLCommon.Bss
{
  public abstract class ServiceViewActionForParent : Activity
  {
    private readonly ServiceViewActionEnum _action;
    public const string SUCCESS_SERVICEVIEW_INFO_TEMPLATE = @"ID: {0} Type: {1} Active: {2} to {3} for AssetID: {4} SN: {5}.";
    public const string COUNT_IS_ZERO_MESSAGE = @"No {0} of ServiceViews for Parent.";
    public const string FAILURE_MESSAGE = @"Failed to execute {0} of ServiceViews for Parent.  See InnerException for details.";
    public const string SUCCESS_MESSAGE = @"{0} of {1} ServiceViews for Parent.";

    protected ServiceViewActionForParent(ServiceViewActionEnum action)
    {
      _action = action;
    }

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<CustomerContext>();

      IList<ServiceViewInfoDto> serviceViews;

      try
      {
        if(_action == ServiceViewActionEnum.Creation)
        {
          serviceViews = Services.ServiceViews().CreateRelationshipServiceViews(context.NewParent.Id, context.Id);
        }
        else
        {
          serviceViews = Services.ServiceViews().TerminateRelationshipServiceViews(context.NewParent.Id, context.Id, DateTime.UtcNow);
        }  
      }
      catch (Exception ex)
      {
        return Exception(ex, FAILURE_MESSAGE, _action);
      }

      if (serviceViews == null || serviceViews.Count == 0)
      {
        return Success(COUNT_IS_ZERO_MESSAGE, _action);
      }

      AddSummary(SUCCESS_MESSAGE, _action, serviceViews.Count);
      foreach (var serviceView in serviceViews)
      {
        AddSummary(SUCCESS_SERVICEVIEW_INFO_TEMPLATE,
          serviceView.ServiceViewId, serviceView.ServiceTypeName,
          serviceView.StartDateKey, serviceView.EndDateKey,
          serviceView.AssetId, serviceView.AssetSerialNumber);
      }
      return Success();
    }
  }
}
