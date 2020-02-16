using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class CustomerRelationshipCreateTests : BssUnitTestBase
  {

    protected Inputs Inputs;
    protected CustomerRelationshipCreate Activity;

    [TestInitialize]
    public void CustomerRelationshipCreateTests_Init()
    {
      Inputs = new Inputs();
      Activity = new CustomerRelationshipCreate();
    }

    [TestMethod]
    public void Execute_ThrowsExceptionWhileCreatingCustomerRelationship_ReturnsExceptionResult()
    {
      var serviceFake = new BssCustomerServiceExceptionFake();
      Services.Customers = () => serviceFake;
      Inputs.Add<CustomerContext>(new CustomerContext());

      var result = Activity.Execute(Inputs) as ExceptionResult;

      Assert.IsNotNull(result);
      StringAssert.Contains(result.Summary, "Failed to create ");
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
    }

    [TestMethod]
    public void Execute_CreateCustomerRelationshipReturnsFalse_ReturnsErrorResult()
    {
      var serviceFake = new BssCustomerServiceFake(false);
      Services.Customers = () => serviceFake;
      Inputs.Add<CustomerContext>(new CustomerContext());

      var result = Activity.Execute(Inputs) as ErrorResult;

      Assert.IsNotNull(result);
      StringAssert.Contains(result.Summary, CustomerRelationshipCreate.RETURN_FALSE_MESSAGE);
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
    }

    [TestMethod]
    public void Execute_NewParentDealer_CreateCustomerRelationshipSuccess_ReturnsMessageUpdatesContext()
    {
      var serviceFake = new BssCustomerServiceFake(true);
      Services.Customers = () => serviceFake;
      var context = new CustomerContext();
      context.NewParent.Id = IdGen.GetId();
      context.NewParent.Type = CustomerTypeEnum.Dealer;
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs);

      string message = string.Format(CustomerRelationshipCreate.MESSAGE,
        context.NewParent.RelationshipType,
        context.New.Type,
        context.Id,
        context.New.Name,
        context.NewParent.Type,
        context.NewParent.Id,
        context.NewParent.Name);

      StringAssert.Contains(result.Summary, message);
      Assert.AreEqual(context.ParentDealer.Id, context.NewParent.Id);
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
    }

    [TestMethod]
    public void Execute_NewParentCustomer_CreateCustomerRelationshipSuccess_ReturnsMessageUpdatesContext()
    {
      var serviceFake = new BssCustomerServiceFake(true);
      Services.Customers = () => serviceFake;
      var context = new CustomerContext();
      context.NewParent.Id = IdGen.GetId();
      context.NewParent.Type = CustomerTypeEnum.Customer;
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs);

      string message = string.Format(CustomerRelationshipCreate.MESSAGE,
        context.NewParent.RelationshipType,
        context.New.Type,
        context.Id,
        context.New.Name,
        context.NewParent.Type,
        context.NewParent.Id,
        context.NewParent.Name);

      StringAssert.Contains(result.Summary, message);
      Assert.AreEqual(context.ParentCustomer.Id, context.NewParent.Id);
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
    }
  }
}
