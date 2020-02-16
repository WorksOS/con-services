using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;
using VSS.Nighthawk.ReferenceIdentifierService.Workers;

namespace VSS.Nighthawk.ReferenceIdentifierService.Tests.WorkerTests
{
  [TestClass]
  public class ServiceIdentifierManagerTests
  {
    [TestMethod]
    public void TestServiceIdentifierManager_Retrieve()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();
      var cim = new ServiceIdentifierManager(_mockStorage.Object);
      cim.Retrieve(new IdentifierDefinition());
      _mockStorage.Verify(o => o.FindServiceReference(It.IsAny<IdentifierDefinition>()), Times.Once());
    }

    [TestMethod]
    public void TestServiceIdentifierManager_Create()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();
      var cim = new ServiceIdentifierManager(_mockStorage.Object);
      cim.Create(new IdentifierDefinition());
      _mockStorage.Verify(o => o.AddServiceReference(It.IsAny<IdentifierDefinition>()), Times.Once());
    }
  }
}
