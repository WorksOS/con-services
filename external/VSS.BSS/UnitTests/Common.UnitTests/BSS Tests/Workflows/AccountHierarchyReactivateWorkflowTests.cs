using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.Nighthawk.NHBssSvc;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Lookups;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class AccountHierarchyReactivateWorkflowTests : UnitTestBase
  {

    protected IWorkflowFactory Factory;
    protected IWorkflowRunner Runner;
    protected WorkflowResult Result;

    [TestInitialize]
    public void AccountHierarchyReactivateWorkflowTests_Init()
    {
      Factory = new BssWorkflowFactory(new BssReference(new Mock<IAssetLookup>().Object, new Mock<ICustomerLookup>().Object));
      Runner = new WorkflowRunner();
    }

    [TestCleanup]
    public void WorkflowTests_Cleanup()
    {
      if (Result == null) return;

      new ConsoleResultProcessor().Process(new AccountHierarchy(), Result);
    }

    [DatabaseTest]
    [TestMethod]
    public void Dealer_Success()
    {
      var bssId = IdGen.GetId().ToString();
      var dealer = Entity.Customer.Dealer.BssId(bssId.ToString()).Save();
      dealer.IsActivated = false;
      Ctx.OpContext.SaveChanges();

      var message = BSS.AHReactivated.ForDealer().BssId(bssId).Build();
      var workflow = Factory.Create(message);
      var result = Runner.Run(workflow);
      var newDealer = (from c in Ctx.OpContext.CustomerReadOnly where c.ID == dealer.ID select c).Single();

      Assert.IsTrue(result.Success);
      Assert.IsTrue(newDealer.IsActivated);
    }

    [DatabaseTest]
    [TestMethod]
    public void Dealer_Currently_Active_SuccessWithWarning()
    {
      var bssId = IdGen.GetId().ToString();
      Entity.Customer.Dealer.BssId(bssId.ToString()).Save();

      var message = BSS.AHReactivated.ForDealer().BssId(bssId).Build();
      var workflow = Factory.Create(message);
      var result = Runner.Run(workflow);

      Assert.IsTrue(result.Success, "Success");
      var warning = result.ActivityResults.First(x => x.Type == ResultType.Warning);
      StringAssert.Contains(warning.Summary, " active");
    }

    [DatabaseTest]
    [TestMethod]
    public void Customer_Success()
    {
      var bssId = IdGen.GetId().ToString();
      var customer = Entity.Customer.EndCustomer.BssId(bssId.ToString()).Save();
      customer.IsActivated = false;
      Ctx.OpContext.SaveChanges();

      var message = BSS.AHReactivated.ForDealer().BssId(bssId).Build();
      var workflow = Factory.Create(message);
      var result = Runner.Run(workflow);

      var newCustomer = (from c in Ctx.OpContext.CustomerReadOnly where c.ID == customer.ID select c).Single();

      Assert.IsTrue(result.Success);
      Assert.IsTrue(newCustomer.IsActivated);
    }

    //[DatabaseTest]
    //[TestMethod]
    //public void AccountHierarchy_Reactivate_Success()
    //{
    //  foreach (AccountHierarchy.BSSCustomerTypeEnum customerType in Enum.GetValues(typeof(AccountHierarchy.BSSCustomerTypeEnum)))
    //  {
    //    foreach (DealerNetworkEnum item in Enum.GetValues(typeof(DealerNetworkEnum)))
    //    {
    //      TestInitializeObjects(customerType: customerType);
    //      DealerCustomerType_DealerNetwork_SuccessScernarios(BSSID: childBSSID, dealerNetwork: item, customerType: customerType);
    //    }
    //  }
    //}

    //[DatabaseTest]
    //[TestMethod]
    //public void AccountHierarchy_Reactivate_BSSIDNotExists_Failure()
    //{
    //  foreach (DealerNetworkEnum item in Enum.GetValues(typeof(DealerNetworkEnum)))
    //  {
    //    foreach (AccountHierarchy.BSSCustomerTypeEnum customerType in Enum.GetValues(typeof(AccountHierarchy.BSSCustomerTypeEnum)))
    //    {
    //      childBSSID = IdGen.GetId();
    //      WorkflowResult result = DealerCustomerType_Failure_Scenarios(dealerNetwork: item, customerType: customerType, existingBSSID: childBSSID);
    //      Assert.IsFalse(result.Success);
    //      Assert.IsTrue(result.Summary.ToUpper().Contains(string.Format(@"{1} not exists for BSSID: ""{0}"".", childBSSID, customerType.ToString()).ToUpper()));
    //    }
    //  }
    //}

    //[DatabaseTest]
    //[TestMethod]
    //public void AccountHierarchy_Reactivate_ParentBSSIDDefined_Failure()
    //{
    //  foreach (DealerNetworkEnum item in Enum.GetValues(typeof(DealerNetworkEnum)))
    //  {
    //    foreach (AccountHierarchy.BSSCustomerTypeEnum customerType in Enum.GetValues(typeof(AccountHierarchy.BSSCustomerTypeEnum)))
    //    {
    //      parentBSSID = IdGen.GetId();
    //      WorkflowResult result = DealerCustomerType_Failure_Scenarios(dealerNetwork: item, customerType: customerType, parentBSSID: parentBSSID);
    //      Assert.IsFalse(result.Success);
    //      Assert.IsTrue(result.Summary.Contains("RelationshipID is not defined."));
    //    }
    //  }
    //}

    //[DatabaseTest]
    //[TestMethod]
    //public void AccountHierarchy_Reactivate_RelationshipIDDefined_Failure()
    //{
    //  foreach (DealerNetworkEnum item in Enum.GetValues(typeof(DealerNetworkEnum)))
    //  {
    //    foreach (AccountHierarchy.BSSCustomerTypeEnum customerType in Enum.GetValues(typeof(AccountHierarchy.BSSCustomerTypeEnum)))
    //    {
    //      WorkflowResult result = DealerCustomerType_Failure_Scenarios(dealerNetwork: item, customerType: customerType, relationshipID: IdGen.GetId());
    //      Assert.IsFalse(result.Success);
    //      Assert.IsTrue(result.Summary.Contains("ParentBSSID is not defined."));
    //    }
    //  }
    //}

    //[DatabaseTest]
    //[TestMethod]
    //public void AccountHierarchy_ExistingCustomerReactivate_ParentBSSIDDefined_Failure()
    //{
    //  foreach (DealerNetworkEnum item in Enum.GetValues(typeof(DealerNetworkEnum)))
    //  {
    //    foreach (AccountHierarchy.BSSCustomerTypeEnum customerType in Enum.GetValues(typeof(AccountHierarchy.BSSCustomerTypeEnum)))
    //    {
    //      TestInitializeObjects(customerType);
    //      WorkflowResult result = DealerCustomerType_Failure_Scenarios(existingBSSID: childBSSID, dealerNetwork: item, customerType: customerType, parentBSSID: IdGen.GetId());
    //      Assert.IsFalse(result.Success);
    //      Assert.IsTrue(result.Summary.Contains("RelationshipID is not defined."));
    //    }
    //  }
    //}

    //[DatabaseTest]
    //[TestMethod]
    //public void AccountHierarchy_ExistingCustomerReactivate_RelationshipIDDefined_Failure()
    //{
    //  foreach (DealerNetworkEnum item in Enum.GetValues(typeof(DealerNetworkEnum)))
    //  {
    //    foreach (AccountHierarchy.BSSCustomerTypeEnum customerType in Enum.GetValues(typeof(AccountHierarchy.BSSCustomerTypeEnum)))
    //    {
    //      TestInitializeObjects(customerType);
    //      WorkflowResult result = DealerCustomerType_Failure_Scenarios(existingBSSID: childBSSID, dealerNetwork: item, customerType: customerType, relationshipID: IdGen.GetId());
    //      Assert.IsFalse(result.Success);
    //      Assert.IsTrue(result.Summary.Contains("ParentBSSID is not defined."));
    //    }
    //  }
    //}

    //[DatabaseTest]
    //[TestMethod]
    //public void AccountHierarchy_ExistingCustomerReactivate_ParentIDAndRelationshipID_Failure()
    //{
    //  foreach (DealerNetworkEnum item in Enum.GetValues(typeof(DealerNetworkEnum)))
    //  {
    //    foreach (AccountHierarchy.BSSCustomerTypeEnum customerType in Enum.GetValues(typeof(AccountHierarchy.BSSCustomerTypeEnum)))
    //    {
    //      TestInitializeObjects(customerType);
    //      WorkflowResult result = DealerCustomerType_Failure_Scenarios(existingBSSID: childBSSID, dealerNetwork: item, customerType: customerType, relationshipID: IdGen.GetId(), defineContact: true);
    //      Assert.IsFalse(result.Success);
    //      Assert.IsTrue(result.Summary.Contains("ParentBSSID is not defined."));
    //      Assert.IsTrue(result.Summary.Contains("PrimaryContact is defined."));
    //    }
    //  }
    //}

    //[DatabaseTest]
    //[TestMethod]
    //public void AccountHierarchy_ExistingCustomerReactivate_ContactDefined_Failure()
    //{
    //  foreach (DealerNetworkEnum item in Enum.GetValues(typeof(DealerNetworkEnum)))
    //  {
    //    foreach (AccountHierarchy.BSSCustomerTypeEnum customerType in Enum.GetValues(typeof(AccountHierarchy.BSSCustomerTypeEnum)))
    //    {
    //      TestInitializeObjects(customerType);
    //      WorkflowResult result = DealerCustomerType_Failure_Scenarios(existingBSSID: childBSSID, dealerNetwork: item, customerType: customerType, defineContact: true);
    //      Assert.IsFalse(result.Success);
    //      Assert.IsTrue(result.Summary.Contains("PrimaryContact is defined."));
    //    }
    //  }
    //}

    //[DatabaseTest]
    //[TestMethod]
    //public void AccountHierarchy_NewCustomerReactivate_ContactDefined_Failure()
    //{
    //  foreach (DealerNetworkEnum item in Enum.GetValues(typeof(DealerNetworkEnum)))
    //  {
    //    foreach (AccountHierarchy.BSSCustomerTypeEnum customerType in Enum.GetValues(typeof(AccountHierarchy.BSSCustomerTypeEnum)))
    //    {
    //      WorkflowResult result = DealerCustomerType_Failure_Scenarios(dealerNetwork: item, customerType: customerType, defineContact: true);
    //      Assert.IsFalse(result.Success);
    //      Assert.IsTrue(result.Summary.Contains("PrimaryContact is defined."));
    //    }
    //  }
    //}

    //[DatabaseTest]
    //[TestMethod]
    //public void AccountHierarchy_NewCustomerReactivate_ParentIDAndRelationshipIDDefined_Failure()
    //{
    //  foreach (DealerNetworkEnum item in Enum.GetValues(typeof(DealerNetworkEnum)))
    //  {
    //    foreach (AccountHierarchy.BSSCustomerTypeEnum customerType in Enum.GetValues(typeof(AccountHierarchy.BSSCustomerTypeEnum)))
    //    {
    //      WorkflowResult result = DealerCustomerType_Failure_Scenarios(dealerNetwork: item, customerType: customerType, relationshipID: IdGen.GetId(), parentBSSID: IdGen.GetId());
    //      Assert.IsFalse(result.Success);
    //      Assert.IsTrue(result.Summary.Contains("ParentBSSID is defined."));
    //    }
    //  }
    //}

    //[DatabaseTest]
    //[TestMethod]
    //public void AccountHierarchy_ExistingCustomerReactivate_ParentIDRelationshipIDDefined_Failure()
    //{
    //  foreach (DealerNetworkEnum item in Enum.GetValues(typeof(DealerNetworkEnum)))
    //  {
    //    foreach (AccountHierarchy.BSSCustomerTypeEnum customerType in Enum.GetValues(typeof(AccountHierarchy.BSSCustomerTypeEnum)))
    //    {
    //      TestInitializeObjects(customerType);
    //      WorkflowResult result = DealerCustomerType_Failure_Scenarios(existingBSSID: childBSSID, dealerNetwork: item, customerType: customerType, relationshipID: IdGen.GetId(), parentBSSID: IdGen.GetId());
    //      Assert.IsFalse(result.Success);
    //      Assert.IsTrue(result.Summary.Contains("ParentBSSID is defined."));
    //    }
    //  }
    //}

    //[DatabaseTest]
    //[TestMethod]
    //public void AccountHierarchy_ExistingCustomerReactivate_ContactParentIDRelationshipIDDefined_Failure()
    //{
    //  foreach (DealerNetworkEnum item in Enum.GetValues(typeof(DealerNetworkEnum)))
    //  {
    //    foreach (AccountHierarchy.BSSCustomerTypeEnum customerType in Enum.GetValues(typeof(AccountHierarchy.BSSCustomerTypeEnum)))
    //    {
    //      TestInitializeObjects(customerType);
    //      WorkflowResult result = DealerCustomerType_Failure_Scenarios(existingBSSID: childBSSID, dealerNetwork: item, customerType: customerType, relationshipID: IdGen.GetId(), parentBSSID: IdGen.GetId(), defineContact: true);
    //      Assert.IsFalse(result.Success);
    //      Assert.IsTrue(result.Summary.Contains("PrimaryContact is defined."));
    //    }
    //  }
    //}

    //[DatabaseTest]
    //[TestMethod]
    //public void AccountHierarchy_NewCustomerReactivate_ContactParentIDRelationshipIDDefined_Failure()
    //{
    //  foreach (DealerNetworkEnum item in Enum.GetValues(typeof(DealerNetworkEnum)))
    //  {
    //    foreach (AccountHierarchy.BSSCustomerTypeEnum customerType in Enum.GetValues(typeof(AccountHierarchy.BSSCustomerTypeEnum)))
    //    {
    //      WorkflowResult result = DealerCustomerType_Failure_Scenarios(dealerNetwork: item, customerType: customerType, relationshipID: IdGen.GetId(), parentBSSID: IdGen.GetId(), defineContact: true);
    //      Assert.IsFalse(result.Success);
    //      Assert.IsTrue(result.Summary.Contains("PrimaryContact is defined."));
    //    }
    //  }
    //}

    //private WorkflowResult DealerCustomerType_Failure_Scenarios(DealerNetworkEnum dealerNetwork,
    //  AccountHierarchy.BSSCustomerTypeEnum customerType,
    //  string hierarchType = "TCS Dealer",
    //  long? parentBSSID = null,
    //  long? relationshipID = null,
    //  long? existingBSSID = null,
    //  bool defineContact = false)
    //{
    //  AccountHierarchy message = new AccountHierarchy
    //  {
    //    TargetStack = "TestStack01",
    //    ActionUTC = DateTime.UtcNow,
    //    CustomerName = "Fake Account",
    //    CustomerType = customerType,
    //    SequenceNumber = IdGen.GetId(),
    //    ControlNumber = IdGen.GetId(),
    //    BSSID = existingBSSID.HasValue ? existingBSSID.Value : IdGen.GetId(),
    //    ParentBSSID = parentBSSID,
    //    RelationshipID = relationshipID,
    //    Action = ActionEnum.Reactivated,
    //    DealerNetwork = customerType == AccountHierarchy.BSSCustomerTypeEnum.DEALER ? dealerNetwork.ToString() : null,
    //    DealerAccountCode = customerType == AccountHierarchy.BSSCustomerTypeEnum.ACCOUNT ? "DAC" : null,
    //    NetworkCustomerCode = customerType == AccountHierarchy.BSSCustomerTypeEnum.ACCOUNT ? "NCC" : null,
    //    NetworkDealerCode = customerType == AccountHierarchy.BSSCustomerTypeEnum.DEALER ? System.IO.Path.GetRandomFileName() : null,
    //    HierarchyType = customerType != AccountHierarchy.BSSCustomerTypeEnum.CUSTOMER ? "TCS Dealer" : "TCS Customer",
    //    contact = defineContact ? new PrimaryContact { } : null
    //  };

    //  var workflow = new BssWorkflowFactory().Create(message);
    //  WorkflowResult result = new WorkflowRunner().Run(workflow);
    //  new ConsoleResultProcessor().Process(message, result);

    //  return result;
    //}

    //private void DealerCustomerType_DealerNetwork_SuccessScernarios(DealerNetworkEnum dealerNetwork, 
    //  AccountHierarchy.BSSCustomerTypeEnum customerType,
    //  long? BSSID = null)
    //{
    //  if (!BSSID.HasValue)
    //    BSSID = IdGen.GetId();
    //  string bssID = BSSID.ToString();
    //  AccountHierarchy message = new AccountHierarchy
    //  {
    //    TargetStack = "TestStack01",
    //    ActionUTC = DateTime.UtcNow,
    //    CustomerName = "Fake Account",
    //    CustomerType = customerType,
    //    contact = null,
    //    SequenceNumber = IdGen.GetId(),
    //    ControlNumber = IdGen.GetId(),
    //    BSSID = BSSID.Value,
    //    Action = ActionEnum.Reactivated,
    //    DealerNetwork = customerType == AccountHierarchy.BSSCustomerTypeEnum.DEALER ? dealerNetwork.ToString() : null,
    //    DealerAccountCode = customerType == AccountHierarchy.BSSCustomerTypeEnum.ACCOUNT ? "DAC" : null,
    //    NetworkCustomerCode = customerType == AccountHierarchy.BSSCustomerTypeEnum.ACCOUNT ? "NCC":null,
    //    NetworkDealerCode = customerType == AccountHierarchy.BSSCustomerTypeEnum.DEALER ? System.IO.Path.GetRandomFileName() : null,
    //    HierarchyType = customerType != AccountHierarchy.BSSCustomerTypeEnum.CUSTOMER ? "TCS Dealer" : "TCS Customer"
    //  };

    //  var workflow = new BssWorkflowFactory().Create(message);
    //  WorkflowResult result = new WorkflowRunner().Run(workflow);
    //  new ConsoleResultProcessor().Process(message, result);
    //  Assert.IsTrue(result.Success);
    //  using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
    //  {
    //    var c = (from customer in ctx.CustomerReadOnly
    //             where customer.BSSID == bssID
    //             select customer.IsActivated);

    //    Assert.AreEqual(1, c.Count());

    //    Assert.IsNotNull(c);

    //    Assert.AreEqual(true, c.First());
    //  }
    //}

    //public void TestInitializeObjects(AccountHierarchy.BSSCustomerTypeEnum customerType)
    //{
    //  childBSSID = IdGen.GetId();

    //  Customer childCustomer = null;
    //  switch (customerType)
    //  {
    //    case AccountHierarchy.BSSCustomerTypeEnum.ACCOUNT:
    //      childCustomer = Entity.Customer.Account.BssId(childBSSID.ToString()).Save();
    //      break;
    //    case AccountHierarchy.BSSCustomerTypeEnum.CUSTOMER:
    //      childCustomer = Entity.Customer.EndCustomer.BssId(childBSSID.ToString()).Save();
    //      break;
    //    case AccountHierarchy.BSSCustomerTypeEnum.DEALER:
    //      childCustomer = Entity.Customer.Dealer.BssId(childBSSID.ToString()).Save();
    //      break;
    //  }
    //}
  }
}
