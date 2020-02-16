using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;
using VSS.Nighthawk.ReferenceIdentifierService.Workers;

namespace VSS.Nighthawk.ReferenceIdentifierService.Tests.WorkerTests
{
  [TestClass]
  public class CustomerIdentifierManagerTests
  {
    [TestMethod]
    public void TestCustomerIdentifierManager_Retrieve()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();
    
      var cim = new CustomerIdentifierManager(_mockStorage.Object);
      cim.Retrieve(new IdentifierDefinition());
      _mockStorage.Verify(o => o.FindCustomerReference(It.IsAny<IdentifierDefinition>()), Times.Once());
    }

    [TestMethod]
    public void TestCustomerIdentifierManager_Create()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();
    
      var cim = new CustomerIdentifierManager(_mockStorage.Object);
      cim.Create(new IdentifierDefinition());
      _mockStorage.Verify(o => o.AddCustomerReference(It.IsAny<IdentifierDefinition>()), Times.Once());
    }

    [TestMethod]
    public void TestCustomerIdentifierManager_Update()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();

      var cim = new CustomerIdentifierManager(_mockStorage.Object);
      cim.Update(new IdentifierDefinition());
      _mockStorage.Verify(o => o.UpdateCustomerReference(It.IsAny<IdentifierDefinition>()), Times.Once());
    }

    [TestMethod]
    public void TestCustomerIdentifierManager_FindeDealers()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();

      var cim = new CustomerIdentifierManager(_mockStorage.Object);
      cim.FindDealers(new List<IdentifierDefinition>(), 1);
      _mockStorage.Verify(o => o.FindDealers(It.IsAny<IList<IdentifierDefinition>>(), It.Is<long>(e=>e==1)), Times.Once());
    }

    [TestMethod]
    public void TestCustomerIdentifierManager_FindCustomerGuidByCustomerID()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();

      var cim = new CustomerIdentifierManager(_mockStorage.Object);
      cim.FindCustomerGuidByCustomerId(1);
      _mockStorage.Verify(o => o.FindCustomerGuidByCustomerId( It.Is<long>(e => e == 1)), Times.Once());
    }
  }
}
