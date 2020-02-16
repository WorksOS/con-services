using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class CustomerUpdateTests : BssUnitTestBase
  {
    protected Inputs Inputs;
    protected CustomerUpdate Activity;

    [TestInitialize]
    public void CustomerUpdateTests_Init()
    {
      Inputs = new Inputs();
      Activity = new CustomerUpdate();
    }

    [TestMethod]
    public void Execute_NoPropertiesModified_ReturnsCancelledMessage()
    {
      var context = new CustomerContext();
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "cancelled");
    }

    [TestMethod]
    public void Execute_CustomerTypeModified_ReturnsWarningAndUpdates()
    {
      var serviceFake = new BssCustomerServiceFake(true);
      Services.Customers = () => serviceFake;
      var context = new CustomerContext();
      context.Type = CustomerTypeEnum.Customer;
      context.New.Type = CustomerTypeEnum.Dealer;
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs);

      Assert.AreEqual(context.New.Type, context.Type);
      StringAssert.Contains(result.Summary, "Updating CustomerType");
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
    }

    [TestMethod]
    public void Execute_DealerNetworkModified_ReturnsWarningAndUpdates()
    {
      var serviceFake = new BssCustomerServiceFake(true);
      Services.Customers = () => serviceFake;
      var context = new CustomerContext();
      context.DealerNetwork = DealerNetworkEnum.None;
      context.New.DealerNetwork = DealerNetworkEnum.CAT;
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs);

      Assert.AreEqual(context.New.DealerNetwork, context.DealerNetwork);
      StringAssert.Contains(result.Summary, "Updating DealerNetwork");
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
    }

    [TestMethod]
    public void Execute_Success_ReturnsMessageUpdateContext()
    {
      var serviceFake = new BssCustomerServiceFake(true);
      Services.Customers = () => serviceFake;
      var context = new CustomerContext();
      context.New.Name = "Some Updated Name";
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Updated ");
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
    }

    [TestMethod]
    public void Execute_ExceptionDuringUpdateCustomerCall_ReturnsExceptionResult()
    {
      var serviceFake = new BssCustomerServiceExceptionFake();
      Services.Customers = () => serviceFake;
      var context = new CustomerContext();
      context.New.Name = "Some Updated Name";
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs) as ExceptionResult;

      Assert.IsNotNull(result);
      StringAssert.Contains(result.Summary, "Failed ");
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
    }

    [TestMethod]
    public void Execute_UpdateCustomerReturnedFalse_ReturnsErrorResult()
    {
      var serviceFake = new BssCustomerServiceFake(false);
      Services.Customers = () => serviceFake;
      var context = new CustomerContext();
      context.New.Name = "Some Updated Name";
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs) as ErrorResult;

      Assert.IsNotNull(result);
      StringAssert.Contains(result.Summary, "unknown reason");
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
    }

    [TestMethod]
    public void Execute_UpdateCustomerUCID_OldNetworkCustomerCodeAdded()
    {
      var serviceFake = new BssCustomerServiceFake(false);
      Services.Customers = () => serviceFake;
      var context = new CustomerContext();
      context.NetworkCustomerCode = "Blah";
      context.New.NetworkCustomerCode = "Blah2";
      Inputs.Add<CustomerContext>(context);
      Activity.Execute(Inputs);
      Assert.AreEqual("Blah", context.OldNetworkCustomerCode);
    }

    [TestMethod]
    public void Execute_UpdateCustomerAccount_OldAccountDealerCodeAdded()
    {
      var serviceFake = new BssCustomerServiceFake(false);
      Services.Customers = () => serviceFake;
      var context = new CustomerContext();
      context.DealerAccountCode = "Blah";
      context.New.DealerAccountCode = "Blah2";
      Inputs.Add<CustomerContext>(context);
      Activity.Execute(Inputs);
      Assert.AreEqual("Blah", context.OldDealerAccountCode);
    }

    [TestMethod]
    public void Execute_UpdateCustomerDealer_OldNetworkDealerCodeAdded()
    {
      var serviceFake = new BssCustomerServiceFake(false);
      Services.Customers = () => serviceFake;
      var context = new CustomerContext();
      context.NetworkDealerCode = "Blah";
      context.New.NetworkDealerCode = "Blah2";
      Inputs.Add<CustomerContext>(context);
      Activity.Execute(Inputs);
      Assert.AreEqual("Blah", context.OldNetworkDealerCode);
    }
  }
}
