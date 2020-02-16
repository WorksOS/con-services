using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon.Bss;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class CustomerDeactivateTests : BssUnitTestBase
  {
    protected Inputs Inputs;
    protected CustomerDeactivate Activity;

    [TestInitialize]
    public void TestInitialize()
    {
      Inputs = new Inputs();
      Activity = new CustomerDeactivate();
    }

    [TestMethod]
    public void Execute_ThrowsExceptionWhileDeactivateCustomer_ReturnsExceptionResult()
    {
      var serviceFake = new BssCustomerServiceExceptionFake();
      Services.Customers = () => serviceFake;
      var context = new CustomerContext { IsActive = true };
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs) as ExceptionResult;

      Assert.IsNotNull(result);
      StringAssert.Contains(result.Summary, "Failed ");
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
    }

    [TestMethod]
    public void Execute_DeactivateCustomerReturnsFalse_ReturnsErrorResult()
    {
      var serviceFake = new BssCustomerServiceFake(false);
      Services.Customers = () => serviceFake;
      var context = new CustomerContext { IsActive = true };
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs) as ErrorResult;

      Assert.IsNotNull(result);
      StringAssert.Contains(result.Summary, CustomerDeactivate.RETURN_FALSE_MESSAGE);
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
    }

    [TestMethod]
    public void Execute_CustomerAlreadyInactive_CancelledMessage()
    {
      var serviceFake = new BssCustomerServiceFake(false);
      Services.Customers = () => serviceFake;
      var context = new CustomerContext {IsActive = false};
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Cancelled ");
      Assert.IsFalse(serviceFake.WasExecuted, "WasExecuted");
    }

    [TestMethod]
    public void Execute_DeactivateCustomerSuccess_ReturnsMessage()
    {
      var serviceFake = new BssCustomerServiceFake(true);
      Services.Customers = () => serviceFake;
      var context = new CustomerContext {IsActive = true};
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Deactivated ");
      Assert.IsFalse(context.IsActive);
    }
  }
}
