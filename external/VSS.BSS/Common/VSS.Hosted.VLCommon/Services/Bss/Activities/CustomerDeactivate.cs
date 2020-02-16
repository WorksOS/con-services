using System;
using VSS.Hosted.VLCommon.Events;

namespace VSS.Hosted.VLCommon.Bss
{
  public class CustomerDeactivate : Activity
  {
    public const string CANCELLED_MESSAGE = @"Cancelled deactivation. {0} is already inactive.";
    public const string RETURN_FALSE_MESSAGE = @"Deactivation of customer came back false for unknown reason.";
    public const string FAILURE_MESSAGE = @"Failed to deactivate {0} Name: {1} for BSSID: {2}.";
    public const string SUCCESS_MESSAGE = @"Deactivated {0} ID: {1} Name: {2} for BSSID: {3}.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<CustomerContext>();

      try
      {
        if (!context.IsActive)
          return Warning(CANCELLED_MESSAGE, context.New.Type);

        var success = Services.Customers().DeactivateCustomer(context.Id);

        if (!success)
          return Error(RETURN_FALSE_MESSAGE);

        context.IsActive = false;
      }
      catch (Exception ex)
      {
        return Exception(ex, FAILURE_MESSAGE,
          context.New.Type,
          context.New.Name,
          context.New.BssId);
      }

      AddEventMessage(inputs, new CustomerDeactivatedEvent
                                                          {
                                                            Source = (int) EventSourceEnum.NhBss,
                                                            CreatedUtc = DateTime.UtcNow,
                                                            CustomerId = context.Id
                                                          });

      return Success(SUCCESS_MESSAGE,
        context.New.Type,
        context.Id,
        context.New.Name,
        context.New.BssId);

    }
  }
}
