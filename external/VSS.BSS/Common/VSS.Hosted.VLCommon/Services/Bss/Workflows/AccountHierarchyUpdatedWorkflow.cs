using VSS.Hosted.VLCommon.Bss.Schema.V2;
namespace VSS.Hosted.VLCommon.Bss
{
  public class AccountHierarchyUpdatedWorkflow : Workflow
  {
    public CustomerContext Context { get { return Inputs.Get<CustomerContext>(); } }

    public AccountHierarchyUpdatedWorkflow(Inputs inputs)
      : base(inputs)
    {
      Do(new Validate<AccountHierarchy>(
         new AccountHierarchyDataContractValidator(),
         new AccountHierarchyValidator()));

      Do(new MapAccountHierarchyToCustomerContext());

      If(() => Context.Exists)
        .ThenDo(new MapCurrentStateToCustomerContext());

      If(() => Context.RelationshipCreatedByCatStore)
        .ThenDo(new CustomerRelationshipUpdate());

      Do(new Validate<CustomerContext>(
         new CustomerContextValidator(),
         new CustomerContextUpdatedValidator()));

      //customer type change
      If(() => Context.Type != Context.New.Type)
        .ThenDo(new Validate<CustomerContext>(new CustomerTypeUpdateValidator()));

      //dealer network change
      If(() => Context.DealerNetwork != Context.New.DealerNetwork)
        .ThenDo(new Validate<CustomerContext>(new DealerNetworkUpdateValidator()));

      TransactionStart();

      Do(new CustomerUpdate());

      //US41091 Disabling Admin User updation
      //If(() => (Context.AdminUser != null && Context.AdminUser.Email != null))
      //  .ThenDo(new AdminUserUpdate());

      If(() => Context.NewParent.Exists && !Context.RelationshipIdExistsForCustomer() && !Context.RelationshipCreatedByCatStore)
        .ThenDo(new ServiceViewCreateForParent(),
                new CustomerRelationshipCreate());

      If(() => string.IsNullOrEmpty(Context.OldDealerAccountCode)
        && !Context.UpdatedNetworkCustomerCode
        && string.IsNullOrEmpty(Context.OldNetworkDealerCode))
        .ThenDo(new CustomerAddReference());

      If(() => !string.IsNullOrEmpty(Context.OldDealerAccountCode)
        || Context.UpdatedNetworkCustomerCode
        || !string.IsNullOrEmpty(Context.OldNetworkDealerCode))
        .ThenDo(new CustomerUpdateReference());

      TransactionCommit();
    }
  }
}