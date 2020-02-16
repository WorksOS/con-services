using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class CustomerCreateTests : BssUnitTestBase
  {

    protected Inputs Inputs;
    protected CustomerCreate Activity;

    [TestInitialize]
    public void CreateCustomerTests_Init()
    {
      Inputs = new Inputs();
      Activity = new CustomerCreate();
    }

    [TestMethod]
    public void Execute_ThrowsExceptionWhileCreatingCustomer_ReturnsExceptionResult()
    {
      BssCustomerServiceExceptionFake serviceFake = new BssCustomerServiceExceptionFake();
      Services.Customers = () => serviceFake;

      Inputs.Add<CustomerContext>(new CustomerContext());

      var result = Activity.Execute(Inputs) as ExceptionResult;

      Assert.IsNotNull(result);
      StringAssert.Contains(result.Summary, "Failed ");
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
    }

    [TestMethod]
    public void Execute_CreateCustomerReturnsNull_ReturnsErrorResult()
    {
      BssCustomerServiceFake serviceFake = new BssCustomerServiceFake((Customer)null);
      Services.Customers = () => serviceFake;

      Inputs.Add<CustomerContext>(new CustomerContext());

      var result = Activity.Execute(Inputs) as ErrorResult;
      Assert.IsNotNull(result);
      StringAssert.Contains(result.Summary, CustomerCreate.CUSTOMER_NULL_MESSAGE);
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
    }

    [TestMethod]
    public void Execute_CustomerCreateSuccess_ReturnsMessageUpdatesCustomerContext()
    {
      Customer newCustomer = new Customer();
      newCustomer.ID = IdGen.GetId();
      newCustomer.BSSID = IdGen.GetId().ToString();
      newCustomer.fk_CustomerTypeID = (int) CustomerTypeEnum.Dealer;
      newCustomer.Name = "NEW_CAT_DEALER";
      newCustomer.fk_DealerNetworkID = (int) DealerNetworkEnum.CAT;
      newCustomer.NetworkDealerCode = "NETWORK_DEALER_CODE";

      BssCustomerServiceFake serviceFake = new BssCustomerServiceFake(newCustomer);
      Services.Customers = () => serviceFake;
      var context = new CustomerContext();
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Created ");
      Assert.AreEqual(newCustomer.ID, context.Id);
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
    }
  }
}
