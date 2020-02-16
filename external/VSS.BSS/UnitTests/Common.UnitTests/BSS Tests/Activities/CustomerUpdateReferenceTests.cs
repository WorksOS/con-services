using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Services.Interfaces;
using VSS.Nighthawk.NHBssSvc;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Lookups;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests.Activities
{
  [TestClass]
  public class CustomerUpdateReferenceTests
  {
    private Inputs Inputs;
    private CustomerUpdateReference Activity;

    [TestInitialize]
    public void CustomerUpdateReference_Init()
    {
      Inputs = new Inputs();
      Activity = new CustomerUpdateReference();
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
      context.OldDealerAccountCode = "111";
      context.CustomerUId = Guid.NewGuid();
      Inputs.Add<CustomerContext>(context);
      Inputs.Add<IBssReference>(bss);
      string message = string.Format("Updated Customer References for ID: {0} Name: {1} for BSSID: {2}.", context.Id, context.Name, context.BssId);
      var result = Activity.Execute(Inputs);
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
      StringAssert.Contains(result.Summary, message);
    }

    [TestMethod]
    public void Execute_SuccessCustomerTest()
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
      context.OldNetworkCustomerCode = "111";
      context.CustomerUId = Guid.NewGuid();
      context.UpdatedNetworkCustomerCode = true;
      Inputs.Add<CustomerContext>(context);
      Inputs.Add<IBssReference>(bss);
      string message = string.Format("Updated Customer References for ID: {0} Name: {1} for BSSID: {2}.", context.Id, context.Name, context.BssId);
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
      context.OldNetworkDealerCode = "111";
      context.CustomerUId = Guid.NewGuid();
      Inputs.Add<CustomerContext>(context);
      Inputs.Add<IBssReference>(bss);
      string message = string.Format("Updated Customer References for ID: {0} Name: {1} for BSSID: {2}.", context.Id, context.Name, context.BssId);
      var result = Activity.Execute(Inputs);
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
      StringAssert.Contains(result.Summary, message);
    }
  }
}
