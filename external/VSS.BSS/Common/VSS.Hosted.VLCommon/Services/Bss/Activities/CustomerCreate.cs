using System;
using VSS.Hosted.VLCommon.Events;

namespace VSS.Hosted.VLCommon.Bss
{
  public class CustomerCreate : Activity
  {
    public const string SUCCESS_MESSAGE = @"Created {0} ID: {1} Name: {2} for BSSID: {3}.";
    public const string CUSTOMER_NULL_MESSAGE = @"Creation of customer came back null for unknown reason.";
    public const string FAILURE_MESSAGE = @"Failed to create {0} Name: {1} for BSSID: {2}.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<CustomerContext>();

      Customer newCustomer;

      try
      {
        newCustomer = Services.Customers().CreateCustomer(context);

        if(newCustomer == null)
          return Error(CUSTOMER_NULL_MESSAGE);
        
        context.Id = newCustomer.ID;
        context.BssId = newCustomer.BSSID;
        context.Name = newCustomer.Name;
        context.DealerNetwork = (DealerNetworkEnum)newCustomer.fk_DealerNetworkID;
        context.NetworkDealerCode = newCustomer.NetworkDealerCode;
        context.NetworkCustomerCode = newCustomer.NetworkCustomerCode;
        context.DealerAccountCode = newCustomer.DealerAccountCode;
        context.IsActive = newCustomer.IsActivated;
        context.Type = (CustomerTypeEnum)newCustomer.fk_CustomerTypeID;
        context.CustomerUId = newCustomer.CustomerUID;
        context.PrimaryEmailContact = newCustomer.PrimaryEmailContact;
        context.FirstName = newCustomer.FirstName;
        context.LastName = newCustomer.LastName;
      }
      catch (Exception ex)
      {
        return Exception(ex, FAILURE_MESSAGE, context.New.Type, context.New.Name, context.New.BssId);
      }

      AddEventMessage(inputs, ActivityHelper.GetCustomerCreatedMessage(context));

      return Success(SUCCESS_MESSAGE,
        (CustomerTypeEnum) newCustomer.fk_CustomerTypeID, 
        newCustomer.ID, 
        newCustomer.Name,
        newCustomer.BSSID);
    }
  }
}
