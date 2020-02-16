using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Workers;

namespace VSS.Nighthawk.ReferenceIdentifierService.Tests.WorkerTests
{
  [TestClass]
  public class StoreLookupManagerTests
  {
    [TestMethod]
    public void FindStoreByCustomerId()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();

      var cim = new StoreLookupManager(_mockStorage.Object);
      cim.FindStoreByCustomerId(1);
      _mockStorage.Verify(o => o.FindStoreByCustomerId(It.IsAny<long>()), Times.Once());
    }
  }
}
