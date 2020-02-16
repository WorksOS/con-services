using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;
using VSS.Nighthawk.ReferenceIdentifierService.Workers;

namespace VSS.Nighthawk.ReferenceIdentifierService.Tests.WorkerTests
{
  [TestClass]
  public class DeviceIdentifierManagerTests
  {
    [TestMethod]
    public void TestDeviceIdentifierManager_Retrieve()
    {
       Mock<IStorage> _mockStorage = new Mock<IStorage>();
    
      var cim = new DeviceIdentifierManager(_mockStorage.Object);
      cim.Retrieve(new IdentifierDefinition());
      _mockStorage.Verify(o => o.FindDeviceReference(It.IsAny<IdentifierDefinition>()), Times.Once());
    }

    [TestMethod]
    public void TestDeviceIdentifierManager_Create()
    {
       Mock<IStorage> _mockStorage = new Mock<IStorage>();
    
      var cim = new DeviceIdentifierManager(_mockStorage.Object);
      cim.Create(new IdentifierDefinition());
      _mockStorage.Verify(o => o.AddDeviceReference(It.IsAny<IdentifierDefinition>()), Times.Once());
    }

    [TestMethod]
    public void TestDeviceIdentifierManager_GetAssociatedAsset()
    {
       Mock<IStorage> _mockStorage = new Mock<IStorage>();
    
      var cim = new DeviceIdentifierManager(_mockStorage.Object);
      var guid = Guid.NewGuid();
      cim.GetAssociatedAsset(guid);
      _mockStorage.Verify(o => o.GetAssociatedAsset(It.Is<Guid>(e => e == guid)), Times.Once());
    }
  }
}
