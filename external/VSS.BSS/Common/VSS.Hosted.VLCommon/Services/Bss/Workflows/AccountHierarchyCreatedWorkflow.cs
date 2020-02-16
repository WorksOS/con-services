using VSS.Hosted.VLCommon.Bss.Schema.V2;

namespace VSS.Hosted.VLCommon.Bss
{
  public class AccountHierarchyCreatedWorkflow : Workflow
  {

    public CustomerContext Context { get { return Inputs.Get<CustomerContext>(); } }

    public AccountHierarchyCreatedWorkflow(Inputs inputs) : base(inputs)
    {
      Do(new Validate<AccountHierarchy>(
         new AccountHierarchyDataContractValidator(),
         new AccountHierarchyValidator()));

      Do(new MapAccountHierarchyToCustomerContext());

      If(() => Context.Exists)
        .ThenDo(new MapCurrentStateToCustomerContext());

      Do(new Validate<CustomerContext>(
         new CustomerContextValidator(), 
         new CustomerContextCreatedValidator()));

      TransactionStart();

      If(() => Context.CreatedByCatStore)
        .ThenDo(new UpdateBssIdsForCustomerCreatedByCatStore())
        .ElseDo(new CustomerCreate());

      //US41091 Disabling admin User create activity 
      //If(() => Context.New.Type != CustomerTypeEnum.Account && !Context.AdminUserExists)
      //  .ThenDo(new AdminUserCreate());

      If(() => Context.NewParent.Exists && !Context.CreatedByCatStore)
        .ThenDo(new ServiceViewCreateForParent(),
                new CustomerRelationshipCreate());

      If(() => Context.NewParent.Exists && Context.CreatedByCatStore)
        .ThenDo(new CustomerRelationshipUpdate());

      Do(new CustomerAddReference());

      TransactionCommit();

    }
  }
}
