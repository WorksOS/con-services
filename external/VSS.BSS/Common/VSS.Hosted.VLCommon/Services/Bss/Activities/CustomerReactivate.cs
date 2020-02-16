using System;
using VSS.Hosted.VLCommon.Events;

namespace VSS.Hosted.VLCommon.Bss
{
  public class CustomerReactivate : Activity
  {
    public const string CANCELLED_MESSAGE = @"Cancelled reactivation. {0} is already active.";
    public const string RETURN_FALSE_MESSAGE = @"reactivation of customer came back false for unknown reason.";
    public const string FAILURE_MESSAGE = @"Failed to reactivate {0} Name: {1} for BSSID: {2}. See InnerException for details.";
    public const string SUCCESS_MESSAGE = @"Reactivated {0} ID: {1} Name: {2} for BSSID: {3}.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<CustomerContext>();

      try
      {
        if (context.IsActive)
          return Warning(CANCELLED_MESSAGE, context.New.Type);

        bool success = Services.Customers().ReactivateCustomer(context.Id);

        if (!success)
          return Error(RETURN_FALSE_MESSAGE);

        context.IsActive = true;
      }
      catch (Exception ex)
      {
        return Exception(ex, FAILURE_MESSAGE, context.New.Type, context.New.Name, context.New.BssId);
      }

      AddEventMessage(inputs, new CustomerReactivatedEvent
                                                          {
                                                            Source = (int) EventSourceEnum.NhBss,
                                                            CreatedUtc = DateTime.UtcNow,
                                                            CustomerId = context.Id
                                                          });

      return Success(SUCCESS_MESSAGE, context.Type, context.Id, context.Name, context.BssId);
    }
  }
}
