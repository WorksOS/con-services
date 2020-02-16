using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class CustomerReactivateTests : BssUnitTestBase
  {
    protected Inputs Inputs;
    protected CustomerReactivate Activity;

    [TestInitialize]
    public void TestInitialize()
    {
      Inputs = new Inputs();
      Activity = new CustomerReactivate();
    }

    [DatabaseTest]
    [TestMethod]
    public void Execute_ThrowsExceptionWhileReactivateCustomer_ReturnsExceptionResult()
    {
      var serviceFake = new BssCustomerServiceExceptionFake();
      Services.Customers = () => serviceFake;
      Inputs.Add<CustomerContext>(new CustomerContext());

      var result = Activity.Execute(Inputs) as ExceptionResult;

      Assert.IsNotNull(result);
      StringAssert.Contains(result.Summary, "Failed ");
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");     
    }

    [DatabaseTest]
    [TestMethod]
    public void Execute_ReactivateCustomerReturnsFalse_ReturnsErrorResult()
    {
      var serviceFake = new BssCustomerServiceFake(false);
      Services.Customers = () => serviceFake;

      Inputs.Add<CustomerContext>(new CustomerContext());

      var result = Activity.Execute(Inputs) as ErrorResult;
      
      Assert.IsNotNull(result);
      StringAssert.Contains(result.Summary, CustomerReactivate.RETURN_FALSE_MESSAGE);
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
    }

    [TestMethod]
    public void Execute_CustomerAlreadyActive_CancelledMessage()
    {
      var serviceFake = new BssCustomerServiceFake(true);
      Services.Customers = () => serviceFake;
      var context = new CustomerContext {IsActive = true};
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Cancelled ");
      Assert.IsFalse(serviceFake.WasExecuted, "WasExecuted");
    }

    [DatabaseTest]
    [TestMethod]
    public void Execute_ReactivateCustomerSuccess_ReturnsMessageUpdatesContext()
    {
      var daoFake = new BssCustomerServiceFake(true);
      Services.Customers = () => daoFake;
      var context = new CustomerContext{IsActive = false};
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Reactivated ");
      Assert.IsTrue(context.IsActive);
    }
  }
}
