using VSS.Hosted.VLCommon.Bss.Schema.V2;
namespace VSS.Hosted.VLCommon.Bss
{
  public class AccountHierarchyDeactivatedWorkflow : Workflow
  {
    public AccountHierarchyDeactivatedWorkflow(Inputs inputs) : base(inputs)
    {
      Do(new Validate<AccountHierarchy>(
        new AccountHierarchyDataContractValidator(),
        new AccountHierarchyValidator()));

      Do(new MapAccountHierarchyToCustomerContext());

      Do(new Validate<CustomerContext>(
        new CustomerContextValidator(),
        new CustomerContextDeactivatedValidator()));

      TransactionStart();

      Do(new CustomerDeactivate());

      TransactionCommit();
    }
  }
}