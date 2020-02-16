using VSS.Hosted.VLCommon.Bss.Schema.V2;
namespace VSS.Hosted.VLCommon.Bss
{
  public class AccountHierarchyReactivatedWorkflow : Workflow
  {
    public AccountHierarchyReactivatedWorkflow(Inputs inputs) : base(inputs)
    {
      Do(new Validate<AccountHierarchy>(
        new AccountHierarchyDataContractValidator(),
        new AccountHierarchyValidator()));

      Do(new MapAccountHierarchyToCustomerContext());

      Do(new Validate<CustomerContext>(
        new CustomerContextReactivatedValidator()));

      TransactionStart();

      Do(new CustomerReactivate());

      TransactionCommit();
    }
  }
}