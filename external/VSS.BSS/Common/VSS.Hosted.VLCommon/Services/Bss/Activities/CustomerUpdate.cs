using System;
using System.Collections.Generic;
using VSS.Hosted.VLCommon.Events;

namespace VSS.Hosted.VLCommon.Bss
{
  public class CustomerUpdate : Activity
  {
    public const string SUCCESS_MESSAGE = @"Updated {0} ID: {1} Name: {2} for BSSID: {3}.";
    public const string CANCELLED_MESSAGE = @"Customer update cancelled. There were no modified properties.";
    public const string RETURNED_FALSE_MESSAGE = @"Update of customer came back false for unknown reason.";
    public const string FAILURE_MESSAGE = @"Failed to update {0} Name: {1} for BSSID: {2}.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var modifiedProperties = new List<Param>();
      var context = inputs.Get<CustomerContext>();
      
      if (!context.Name.IsStringEqual(context.New.Name))
        modifiedProperties.Add(new Param { Name = "Name", Value = context.New.Name });

      if (context.Type != context.New.Type)
      {
        AddWarning("Updating CustomerType.");
        modifiedProperties.Add(new Param { Name = "fk_CustomerTypeID", Value = (int)context.New.Type });
      }

      if (context.DealerNetwork != context.New.DealerNetwork)
      {
        AddWarning("Updating DealerNetwork.");
        modifiedProperties.Add(new Param { Name = "fk_DealerNetworkID", Value = (int)context.New.DealerNetwork });
      }

      //needs to update dealer reference and ALL Account references
      if (!context.NetworkDealerCode.IsStringEqual(context.New.NetworkDealerCode))
      {
        context.OldNetworkDealerCode = context.NetworkDealerCode;
        modifiedProperties.Add(new Param { Name = "NetworkDealerCode", Value = context.New.NetworkDealerCode });
      }

      //Needs to update customer References
      if (!context.NetworkCustomerCode.IsStringEqual(context.New.NetworkCustomerCode))
      {
        context.UpdatedNetworkCustomerCode = true;
        context.OldNetworkCustomerCode = context.NetworkCustomerCode;
        modifiedProperties.Add(new Param { Name = "NetworkCustomerCode", Value = context.New.NetworkCustomerCode });
      }

      //Needs to update account reference
      if (!context.DealerAccountCode.IsStringEqual(context.New.DealerAccountCode))
      {
        context.OldDealerAccountCode = context.DealerAccountCode;
        modifiedProperties.Add(new Param { Name = "DealerAccountCode", Value = context.New.DealerAccountCode });
      }

      if (!context.PrimaryEmailContact.IsStringEqual(context.New.PrimaryEmailContact))
      {
        context.OldPrimaryEmailContact = context.PrimaryEmailContact;
        modifiedProperties.Add(new Param { Name = "PrimaryEmailContact", Value = context.New.PrimaryEmailContact });
      }
      if (!context.FirstName.IsStringEqual(context.New.FirstName))
      {
        context.OldFirstName = context.FirstName;
        modifiedProperties.Add(new Param { Name = "FirstName", Value = context.New.FirstName });
      }
      if (!context.LastName.IsStringEqual(context.New.LastName))
      {
        context.OldLastName = context.LastName;
        modifiedProperties.Add(new Param { Name = "LastName", Value = context.New.LastName });
      }
      if (modifiedProperties.Count == 0)
        return Cancelled(CANCELLED_MESSAGE);

      try
      {
        bool success = Services.Customers().UpdateCustomer(context.Id, modifiedProperties);

        if (!success)
          return Error(RETURNED_FALSE_MESSAGE);

        context.Name = context.New.Name;
        context.Type = context.New.Type;
        context.DealerNetwork = context.New.DealerNetwork;
        context.NetworkDealerCode = context.New.NetworkDealerCode;
        context.NetworkCustomerCode = context.New.NetworkCustomerCode;
        context.DealerAccountCode = context.New.DealerAccountCode;
        context.PrimaryEmailContact = context.New.PrimaryEmailContact;
        context.FirstName = context.New.FirstName;
        context.LastName = context.New.LastName;
      }
      catch (Exception ex)
      {
        return Exception(ex, FAILURE_MESSAGE, 
          context.New.Type, 
          context.New.Name, 
          context.New.BssId);
      }

      AddEventMessage(inputs, ActivityHelper.GetCustomerUpdatedMessage(context));

      AddSummary(SUCCESS_MESSAGE, context.Type, context.Id, context.New.Name, context.BssId);
      modifiedProperties.ForEach(x => AddSummary("Modified {0}: {1}", x.Name, x.Value));
      return Success();
    }
  }
}
