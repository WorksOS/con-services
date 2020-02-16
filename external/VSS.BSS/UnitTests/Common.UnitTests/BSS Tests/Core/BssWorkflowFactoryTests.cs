using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.Nighthawk.NHBssSvc;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Lookups;

namespace UnitTests.BSS_Tests
{
  // For use in testing factory initialization.
  public class MessageStub { public string Action { get; set; } }
  public class MessageStubActionWorkflow : Workflow 
  {
    public MessageStubActionWorkflow() : base(null) { }
  }
  public class CantInitThisType 
  {
    public string Action { get { return ""; } } 
  }
  public class NotIWorkflow
  {
    public string Action { get { return ""; } }
  }
  public class NotIWorkflowWorkflow
  {
    public NotIWorkflowWorkflow(Inputs inputs) { }
  }

  [TestClass]
  public class BssWorkflowFactoryTests : BssUnitTestBase
  {
    /*
		 * The folowing Two tests are now untestable with the addition of IsValidActionForMessage method
     * However, the code that these method tested remains inplace in the case a developer adds a workflow
     * and does not put it in the right spot or inherit from IWorkflow.
		 */

    #region Old Unused Tests
    // [TestMethod]
    //public void Failed_to_initialize_factory_could_not_load_type()
    //{
    //  bool thrown = false;
    //  Type type  = typeof(CantInitThisType);
    //  string workflowName1 = string.Format("{0}.{1}Workflow", type.Namespace, type.Name);
    //  string workflowName2 = string.Format("{0}.{1}Workflow", typeof(WorkflowFactory).Namespace, type.Name);
    //  try
    //  {
    //    new BssWorkflowFactory().Create(new CantInitThisType());
    //  }
    //  catch (Exception ex)
    //  {
    //    string message = string.Format(CoreConstants.WORKFLOW_CANNOT_BE_INITIALIZED, typeof(BssWorkflowFactory).FullName, workflowName1, workflowName2);
    //    Assert.AreEqual(message, ex.Message);
    //    thrown = true;
    //  }

    //  Assert.IsTrue(thrown);
    //}

    //[TestMethod]
    //public void Failed_To_Initialize_Factory_Does_Not_Implement_IWorkflow()
    //{
    //  bool thrown = false;
    //  Type type = typeof(NotIWorkflow);
    //  string workflowName = string.Format("{0}.{1}Workflow", type.Namespace, type.Name);
    //  try
    //  {
    //    new BssWorkflowFactory().Create(new NotIWorkflow());
    //  }
    //  catch (Exception ex)
    //  {
    //    string message = string.Format(CoreConstants.WORKFLOW_NOT_IWORKFLOW, workflowName, typeof(IWorkflow));
    //    Assert.AreEqual(message, ex.Message);
    //    thrown = true;
    //  }

    //  Assert.IsTrue(thrown);
    //}

    #endregion

    #region Invalid Action Tests
    [TestMethod]
    public void Create_AccountHierarchy_ActionIsInvalid_ReturnsActionInvalidWorkflow()
    {
      var message = new AccountHierarchy { Action = ActionEnum.Swapped.ToString() };

      var workflow = new BssWorkflowFactory(new BssReference(new Mock<IAssetLookup>().Object, new Mock<ICustomerLookup>().Object)).Create(message);

      Assert.IsInstanceOfType(workflow, typeof(InvalidActionWorkflow));
    }
    #endregion

    #region AccountHierarchy Workflow Creation Tests

    [TestMethod]
    public void Create_AccountHierarchy_ActionEqualsCreated_ReturnsAccountHierarchyCreatedWorkflow()
    {
      var account = new AccountHierarchy { Action = ActionEnum.Created.ToString() };
      var workflow = new BssWorkflowFactory(new BssReference(new Mock<IAssetLookup>().Object, new Mock<ICustomerLookup>().Object)).Create(account);

      Assert.IsInstanceOfType(workflow, typeof(AccountHierarchyCreatedWorkflow));
    }

    [TestMethod]
    public void Create_AccountHierarchy_ActionEqualsUpdated_ReturnsAccountHierarchyUpdatedWorkflow()
    {
      var account = new AccountHierarchy { Action = ActionEnum.Updated.ToString() };
      var workflow = new BssWorkflowFactory(new BssReference(new Mock<IAssetLookup>().Object, new Mock<ICustomerLookup>().Object)).Create(account);

      Assert.IsInstanceOfType(workflow, typeof(AccountHierarchyUpdatedWorkflow));
    }

    [TestMethod]
    public void Create_AccountHierarchy_ActionEqualsDeleted_ReturnsAccountHierarchyDeletedWorkflow()
    {
      var account = new AccountHierarchy { Action = ActionEnum.Deleted.ToString() };
      var workflow = new BssWorkflowFactory(new BssReference(new Mock<IAssetLookup>().Object, new Mock<ICustomerLookup>().Object)).Create(account);

      Assert.IsInstanceOfType(workflow, typeof(AccountHierarchyDeletedWorkflow));
    }

    [TestMethod]
    public void Create_AccountHierarchy_ActionEqualsDeactivated_ReturnsAccountHierarchyDeactivatedWorkflow()
    {
      var account = new AccountHierarchy { Action = ActionEnum.Deactivated.ToString() };
      var workflow = new BssWorkflowFactory(new BssReference(new Mock<IAssetLookup>().Object, new Mock<ICustomerLookup>().Object)).Create(account);

      Assert.IsInstanceOfType(workflow, typeof(AccountHierarchyDeactivatedWorkflow));
    }

    [TestMethod]
    public void Create_AccountHierarchy_ActionEqualsReactivated_ReturnsAccountHierarchyReactivatedWorkflow()
    {
      var account = new AccountHierarchy { Action = ActionEnum.Reactivated.ToString() };
      var workflow = new BssWorkflowFactory(new BssReference(new Mock<IAssetLookup>().Object, new Mock<ICustomerLookup>().Object)).Create(account);

      Assert.IsInstanceOfType(workflow, typeof(AccountHierarchyReactivatedWorkflow));
    }

    #endregion

    #region InstallBase Workflow Creation Tests

    [TestMethod]
    public void Create_InstallBase_ActionEqualsCreated_ReturnsInstallBaseCreatedWorkflow()
    {
      var installBase = new InstallBase{ Action = ActionEnum.Created.ToString() };
      var workflow = new BssWorkflowFactory(new BssReference(new Mock<IAssetLookup>().Object, new Mock<ICustomerLookup>().Object)).Create(installBase);

      Assert.IsInstanceOfType(workflow, typeof(InstallBaseCreatedWorkflow));
    }

    #endregion

  }
}
