using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Services.Interfaces;
using VSS.Nighthawk.NHBssSvc;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Lookups;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests.Activities
{
  [TestClass]
  public class CustomerCreateReferenceTests
  {
    protected Inputs Inputs;
    protected CustomerAddReference Activity;

    [TestInitialize]
    public void CustomerCreateReference_Init()
    {
      Inputs = new Inputs();
      Activity = new CustomerAddReference();
    }


    [TestMethod]
    public void Execute_SuccessAccountTest()
    {
      var serviceFake = new BssCustomerServiceFake(true);
      Services.Customers = () => serviceFake;
      var context = new CustomerContext();
      var bss = new BssReference(new Mock<IAssetLookup>().Object, new Mock<ICustomerLookup>().Object);
      context.Id = IdGen.GetId();
      context.Name = "NAME";
      context.BssId = "BSSID";
      context.ParentDealer.Id = IdGen.GetId();
      context.ParentDealer.Type = CustomerTypeEnum.Dealer;
      context.Type = CustomerTypeEnum.Account;
      context.ParentDealer.DealerNetwork = DealerNetworkEnum.CAT;
      context.ParentCustomer.Id = IdGen.GetId();
      context.ParentCustomer.Type = CustomerTypeEnum.Customer;
      context.ParentCustomer.CustomerUId = Guid.NewGuid();
      context.ParentCustomer.BssId = "44";
      context.NetworkDealerCode = "25";
      context.DealerAccountCode = "25";
      context.NetworkCustomerCode = "25";
      context.CustomerUId = Guid.NewGuid();
      Inputs.Add<CustomerContext>(context);
      Inputs.Add<IBssReference>(bss);
      string message = string.Format("Created Customer References for ID: {0} Name: {1} for BSSID: {2}.", context.Id, context.Name, context.BssId);
      var result = Activity.Execute(Inputs);
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
      StringAssert.Contains(result.Summary, message);
    }

    [TestMethod]
    public void Execute_SuccessDealerTest()
    {
      var serviceFake = new BssCustomerServiceFake(true);
      Services.Customers = () => serviceFake;
      var context = new CustomerContext();
      var bss = new BssReference(new Mock<IAssetLookup>().Object, new Mock<ICustomerLookup>().Object);
      context.Id = IdGen.GetId();
      context.Name = "NAME";
      context.BssId = "BSSID";
      context.Type = CustomerTypeEnum.Dealer;
      context.CustomerUId = Guid.NewGuid();
      context.DealerNetwork = DealerNetworkEnum.TRMB;
      context.NetworkDealerCode = "25";
      
      Inputs.Add<CustomerContext>(context);
      Inputs.Add<IBssReference>(bss);
      string message = string.Format("Created Customer References for ID: {0} Name: {1} for BSSID: {2}.", context.Id, context.Name, context.BssId);
      var result = Activity.Execute(Inputs);
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
      StringAssert.Contains(result.Summary, message);
    }

    [TestMethod]
    public void Execute_ThrowException_ReturnExceptionResult()
    {
      var serviceFake = new BssCustomerServiceExceptionFake();
      Services.Customers = () => serviceFake;

      var context = new CustomerContext();
      context.Id = IdGen.GetId();
      context.Name = "NAME";
      context.BssId = "BSSID";
      context.ParentDealer.Id = IdGen.GetId();
      context.ParentDealer.Type = CustomerTypeEnum.Dealer;
      context.Type = CustomerTypeEnum.Account;
      context.ParentDealer.DealerNetwork = DealerNetworkEnum.CAT;
      context.ParentCustomer.Id = IdGen.GetId();
      context.ParentCustomer.Type = CustomerTypeEnum.Customer;

      Inputs.Add<CustomerContext>(context);
      Inputs.Add<IBssReference>(new BssReference(new Mock<IAssetLookup>().Object, new Mock<ICustomerLookup>().Object));

      var activityResult = Activity.Execute(Inputs);

      Assert.IsTrue(serviceFake.WasExecuted, "AddCustomerReference method should have been invoked.");
      StringAssert.Contains(activityResult.Summary, "Failed");
    }
  }
}
