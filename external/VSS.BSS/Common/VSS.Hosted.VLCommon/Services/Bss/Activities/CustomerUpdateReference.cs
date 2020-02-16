using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Services.Bss;
using VSS.Hosted.VLCommon.Services.Interfaces;

namespace VSS.Hosted.VLCommon.Bss
{
  public class CustomerUpdateReference : Activity
  {
    public const string SUCCESS_MESSAGE = @"Updated Customer References for ID: {0} Name: {1} for BSSID: {2}.";
    public const string FailureMessage = @"Failed to Update CustomerReferences.  Message: {0}";
    private const string accountAlias = "DealerCode_DCN";
    private const string accountKeyFormat = @"{0}_{1}";
    private const string customerAlias = "UCID";
    private const string dealerAlias = "DealerCode";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<CustomerContext>();
      var updateBssReference = inputs.Get<IBssReference>();

      try
      {
        if (!string.IsNullOrEmpty(context.OldDealerAccountCode))
        {
          //update account reference
          string newDealerAccountReference = string.Format(accountKeyFormat, context.ParentDealer.NetworkDealerCode, context.DealerAccountCode);
          Services.Customers().UpdateCustomerReference(updateBssReference, accountAlias, newDealerAccountReference, context.CustomerUId.Value);
        }
        if (context.UpdatedNetworkCustomerCode && (string.IsNullOrEmpty(context.OldNetworkDealerCode)))
        { 
          //update customer reference
          Guid customerUid;
          if (context.ParentCustomer.CustomerUId.HasValue)
            customerUid = context.ParentCustomer.CustomerUId.Value;
          else
            customerUid = context.CustomerUId.Value;
          Services.Customers().UpdateCustomerReference(updateBssReference, customerAlias, context.NetworkCustomerCode, context.CustomerUId.Value);
        }
        if(!string.IsNullOrEmpty(context.OldNetworkDealerCode))
        {
          //update Dealer reference
          Services.Customers().UpdateCustomerReference(updateBssReference, dealerAlias, context.NetworkDealerCode, context.CustomerUId.Value);
          //find all the account references for the old NetworkDealerCode
          IList<AccountInfo> accounts = Services.Customers().GetDealerAccounts(updateBssReference, context.CustomerUId.Value);
          //loop through and update all account references with new network dealer code
          foreach(var account in accounts)
          {
            string newDealerAccountReference = string.Format(accountKeyFormat, context.NetworkDealerCode, account.DealerAccountCode);
            Services.Customers().UpdateCustomerReference(updateBssReference,accountAlias, newDealerAccountReference, account.CustomerUid);
          }
        }
      }
      catch (Exception ex)
      {
        return Notify(ex, FailureMessage, ex.Message);
      }

      return Success(SUCCESS_MESSAGE, context.Id, context.Name, context.BssId);
    }
  }
}
