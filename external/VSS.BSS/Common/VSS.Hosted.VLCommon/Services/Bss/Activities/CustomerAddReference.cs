using System;
using VSS.Hosted.VLCommon.Services.Interfaces;

namespace VSS.Hosted.VLCommon.Bss
{
  public class CustomerAddReference : Activity
  {
    public const string SUCCESS_MESSAGE = @"Created Customer References for ID: {0} Name: {1} for BSSID: {2}.";
    public const string FailureMessage = @"Failed to create CustomerReferences.  Message: {0}";
    private const string accountAlias = "DealerCode_DCN";
    private const string accountKeyFormat = @"{0}_{1}";
    private const string customerAlias = "UCID";
    private const string dealerAlias = "DealerCode";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<CustomerContext>();
      var addBssReference = inputs.Get<IBssReference>();

      try
      {

        DealerNetworkEnum dealerNetwork = context.DealerNetwork == DealerNetworkEnum.None ? context.ParentDealer.DealerNetwork : context.DealerNetwork;
        if (NeedsStore(context.Id, dealerNetwork, context.BssId))
        {
          Services.Customers().CreateStore(context.Id);
        }
        //when customer is first created we do not have dealer network so we create the store for cat dealer network when setting up the account
        if (context.Type == CustomerTypeEnum.Account && context.ParentCustomer != null && context.ParentCustomer.Id != 0
          && NeedsStore(context.ParentCustomer.Id, dealerNetwork, context.ParentCustomer.BssId))
        {
          Services.Customers().CreateStore(context.ParentCustomer.Id);
        }

        //Add Trimble Store Reference
        CreateReference(addBssReference, context, (long)StoreEnum.Trimble);
        //create cat reference if needed
        if (dealerNetwork == DealerNetworkEnum.CAT && !context.CreatedByCatStore)
          CreateReference(addBssReference, context, (long)StoreEnum.CAT);
      }
      catch(Exception ex)
      {
        return Notify(ex, FailureMessage, ex.Message);
      }

      return Success(SUCCESS_MESSAGE, context.Id, context.Name, context.BssId);
    }

    private void CreateReference(IBssReference addBssReference, CustomerContext context, long storeId)
    {
      if (context.Type == CustomerTypeEnum.Account)
      {
        if (context.CustomerUId.HasValue && !string.IsNullOrEmpty(context.ParentDealer.NetworkDealerCode) && !string.IsNullOrEmpty(context.DealerAccountCode) && (!context.BssId.StartsWith("-") || storeId == (long)StoreEnum.Trimble))
        {
          Services.Customers().AddCustomerReference(addBssReference, storeId, accountAlias, string.Format(accountKeyFormat, context.ParentDealer.NetworkDealerCode, context.DealerAccountCode), context.CustomerUId.Value);
        }

        if (context.ParentCustomer.CustomerUId.HasValue && !string.IsNullOrEmpty(context.NetworkCustomerCode) && (!context.ParentCustomer.BssId.StartsWith("-") || storeId == (long)StoreEnum.Trimble))
        {
          
          Services.Customers().AddCustomerReference(addBssReference, storeId, customerAlias, context.NetworkCustomerCode, context.ParentCustomer.CustomerUId.Value);
        }
      }
      else if (context.Type == CustomerTypeEnum.Dealer && context.CustomerUId.HasValue && !string.IsNullOrEmpty(context.NetworkDealerCode) && (!context.BssId.StartsWith("-") || storeId == (long)StoreEnum.Trimble))
      {
        Services.Customers().AddCustomerReference(addBssReference, storeId, dealerAlias, context.NetworkDealerCode, context.CustomerUId.Value);
      }
    }

    private bool NeedsStore(long customerID, DealerNetworkEnum dealerNetwork, string bssId)
    {
      if (dealerNetwork == DealerNetworkEnum.CAT && !string.IsNullOrEmpty(bssId) && !bssId.StartsWith("-"))
        return !Services.Customers().HasStore(customerID);

      return false;
    }
  }
}