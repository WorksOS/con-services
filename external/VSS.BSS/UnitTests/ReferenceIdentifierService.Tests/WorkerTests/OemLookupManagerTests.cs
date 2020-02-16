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
  public class OemLookupManagerTests
  {
    [TestMethod]
    public void FindOemIdentifierByCustomerId_Test()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();

      var cim = new OemLookupManager(_mockStorage.Object);
      cim.FindOemIdentifierByCustomerId(1);
      _mockStorage.Verify(o => o.FindOemIdentifierByCustomerId(It.IsAny<long>()), Times.Once());
    }
  }
}
