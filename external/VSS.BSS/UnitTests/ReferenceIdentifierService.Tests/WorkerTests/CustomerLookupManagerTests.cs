using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;
using VSS.Nighthawk.ReferenceIdentifierService.Workers;

namespace VSS.Nighthawk.ReferenceIdentifierService.Tests.WorkerTests
{
  [TestClass]
  public class CustomerLookupManagerTests
  {
    [TestMethod]
    public void TestCustomerLookupManager_FindeDealers()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();

      var cim = new CustomerLookupManager(_mockStorage.Object);
      cim.FindDealers(new List<IdentifierDefinition>(), 1);
      _mockStorage.Verify(o => o.FindDealers(It.IsAny<IList<IdentifierDefinition>>(), It.Is<long>(e => e == 1)), Times.Once());
    }

    [TestMethod]
    public void TestCustomerIdentifierManager_FindCustomerGuidByCustomerID()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();

      var cim = new CustomerLookupManager(_mockStorage.Object);
      cim.FindCustomerGuidByCustomerId(1);
      _mockStorage.Verify(o => o.FindCustomerGuidByCustomerId(It.Is<long>(e => e == 1)), Times.Once());
    }

    [TestMethod]
    public void TestCustomerIdentifierManager_FindAllCustomersForService()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();

      var cim = new CustomerLookupManager(_mockStorage.Object);
      cim.FindAllCustomersForService(Guid.NewGuid());
      _mockStorage.Verify(o => o.FindAllCustomersForService(It.IsAny<Guid>()), Times.Once());
    }

    [TestMethod]
    public void TestCustomerIdentifierManager_FindCustomerParent()
    {
      var guid = Guid.NewGuid();
      var customerType = CustomerTypeEnum.Dealer;
      Mock<IStorage> _mockStorage = new Mock<IStorage>();

      var cim = new CustomerLookupManager(_mockStorage.Object);
      cim.FindCustomerParent(guid, customerType);
      _mockStorage.Verify(o => o.FindCustomerParent(guid, customerType), Times.Once());
    }

    [TestMethod]
    public void TestCustomerIdentifierManager_FindAccountsForDealer()
    {
      var guid = Guid.NewGuid();
      Mock<IStorage> _mockStorage = new Mock<IStorage>();

      var cim = new CustomerLookupManager(_mockStorage.Object);
      cim.FindAccountsForDealer(guid);
      _mockStorage.Verify(o => o.FindAccountsForDealer(guid), Times.Once());
    }
  }
}
