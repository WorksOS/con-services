using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;
using VSS.Nighthawk.ReferenceIdentifierService.Workers;

namespace VSS.Nighthawk.ReferenceIdentifierService.Tests.WorkerTests
{
  [TestClass]
  public class AssetIdentifierManagerTests
  {
    [TestMethod]
    public void TestAssetIdentifierManager_Retrieve()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();

      var cim = new AssetIdentifierManager(_mockStorage.Object);
      cim.Retrieve(new IdentifierDefinition());
      _mockStorage.Verify(o => o.FindAssetReference(It.IsAny<IdentifierDefinition>()), Times.Once());
    }

    [TestMethod]
    public void TestAssetIdentifierManager_Create()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();

      var cim = new AssetIdentifierManager(_mockStorage.Object);
      cim.Create(new IdentifierDefinition());
      _mockStorage.Verify(o => o.AddAssetReference(It.IsAny<IdentifierDefinition>()), Times.Once());
    }

    [TestMethod]
    public void TestAssetIdentifierManager_GetAssociatedDevices()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();

      var cim = new AssetIdentifierManager(_mockStorage.Object);
      var guid = Guid.NewGuid();
      cim.GetAssociatedDevices(guid);
      _mockStorage.Verify(o => o.GetAssociatedDevices(It.Is<Guid>(e=>e==guid)), Times.Once());
    }

    [TestMethod]
    public void TestAssetIdentifierManager_FindOwner()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();

      var cim = new AssetIdentifierManager(_mockStorage.Object);
      var guid = Guid.NewGuid();
      cim.FindOwner(guid);
      _mockStorage.Verify(o => o.FindOwner(It.Is<Guid>(e => e == guid)), Times.Once());
    }
  }
}
