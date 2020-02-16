using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Workers;

namespace VSS.Nighthawk.ReferenceIdentifierService.Tests.WorkerTests
{
  [TestClass]
  public class ServiceLookupManagerTests
  {
    [TestMethod]
    public void GetAssetActiveServices_Test()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();

      var cim = new ServiceLookupManager(_mockStorage.Object);
      cim.GetAssetActiveServices(Guid.NewGuid());
      _mockStorage.Verify(o => o.GetAssetActiveServices(It.IsAny<Guid>()), Times.Once());
    }

    [TestMethod]
    public void GetAssetActiveServices_SerialMake_Test()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();

      var cim = new ServiceLookupManager(_mockStorage.Object);
      cim.GetAssetActiveServices("Serial", "Make");
      _mockStorage.Verify(o => o.GetAssetActiveServices(It.Is<string>(s=>s=="Serial"), It.Is<string>(m=>m=="Make")), Times.Once());
    }

    [TestMethod]
    public void GetDeviceActiveServices_Test()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();

      var cim = new ServiceLookupManager(_mockStorage.Object);
      cim.GetDeviceActiveServices("Serial", DeviceTypeEnum.SNM451);
      _mockStorage.Verify(o => o.GetDeviceActiveServices(It.Is<string>(s => s == "Serial"), It.Is<DeviceTypeEnum>(m => m == DeviceTypeEnum.SNM451)), Times.Once());
    }

  }
}
