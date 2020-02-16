using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class CustomerRelationshipDeleteTests : BssUnitTestBase
  {
    protected Inputs Inputs;
    protected CustomerRelationshipDelete Activity;

    [TestInitialize]
    public void TestInitialize()
    {
      Inputs = new Inputs();
      Activity = new CustomerRelationshipDelete();
    }

    [TestMethod]
    public void Execute_CanNotFindParentToDelete_ReturnsErrorResult()
    {
      var serviceFake = new BssCustomerServiceFake(true);
      Services.Customers = () => serviceFake;
      var context = GetCustomerContext();
      context.NewParent.Type = CustomerTypeEnum.Account;
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs) as WarningResult;

      Assert.IsNotNull(result);
      StringAssert.Contains(result.Summary, CustomerRelationshipDelete.CANCELLED_MESSAGE);
      Assert.IsFalse(serviceFake.WasExecuted, "WasExecuted");
    }

    [TestMethod]
    public void Execute_ThrowsExceptionWhileDeleteCustomerRelationship_ReturnsExceptionResult()
    {
      var serviceFake = new BssCustomerServiceExceptionFake();
      Services.Customers = () => serviceFake;
      var context = GetCustomerContext();
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs) as ExceptionResult;

      Assert.IsNotNull(result);
      StringAssert.Contains(result.Summary, "Failed ");
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
    }

    [TestMethod]
    public void Execute_DeleteCustomerRelationshipReturnsFalse_ReturnsErrorResult()
    {
      var serviceFake = new BssCustomerServiceFake(false);
      Services.Customers = () => serviceFake;
      var context = GetCustomerContext();
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs) as  ErrorResult;

      Assert.IsNotNull(result);
      StringAssert.Contains(result.Summary, CustomerRelationshipDelete.RETURN_FALSE_MESSAGE);
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
    }

    [TestMethod]
    public void Execute_NewParentDealer_DeleteCustomerRelationshipSuccess_ReturnsMessageUpdatesContext()
    {
      var daoFake = new BssCustomerServiceFake(true);
      Services.Customers = () => daoFake;
      var context = GetCustomerContext();
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Deleted CustomerRelationship ");
      Assert.IsFalse(context.ParentDealer.Exists);
    }

    [TestMethod]
    public void Execute_NewParentCustomer_DeleteCustomerRelationshipSuccess_ReturnsMessageUpdatesContext()
    {
      var daoFake = new BssCustomerServiceFake(true);
      Services.Customers = () => daoFake;
      var context = new CustomerContext();
      context.NewParent.Type = CustomerTypeEnum.Customer;
      context.ParentCustomer.Id = IdGen.GetId();
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Deleted CustomerRelationship ");
      Assert.IsFalse(context.ParentCustomer.Exists);
    }

    private CustomerContext GetCustomerContext()
    {
      var context = new CustomerContext();
      context.NewParent.Id = IdGen.GetId();
      context.NewParent.Type = CustomerTypeEnum.Dealer;
      context.ParentDealer.Id = IdGen.GetId();
      return context;
    }
  }
}
