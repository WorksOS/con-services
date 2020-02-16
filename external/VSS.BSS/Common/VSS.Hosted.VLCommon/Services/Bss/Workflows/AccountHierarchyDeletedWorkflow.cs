using VSS.Hosted.VLCommon.Bss.Schema.V2;
namespace VSS.Hosted.VLCommon.Bss
{
  public class AccountHierarchyDeletedWorkflow : Workflow
  {
    public CustomerContext Context { get { return Inputs.Get<CustomerContext>(); } }

    public AccountHierarchyDeletedWorkflow(Inputs inputs) : base(inputs)
    {
      Do(new Validate<AccountHierarchy>(
        new AccountHierarchyDataContractValidator(),
        new AccountHierarchyValidator(),
        new AccountHierarchyDeletedValidator()));

      Do(new MapAccountHierarchyToCustomerContext());

      If(() => Context.Exists)
        .ThenDo(new MapCurrentStateToCustomerContext());

      Do(new Validate<CustomerContext>(
        new CustomerContextValidator(),
        new CustomerContextDeletedValidator()));

      TransactionStart();

      Do(new ServiceViewTerminationForParent(),
         new CustomerRelationshipDelete());
      
      TransactionCommit();
    }
  }
}
