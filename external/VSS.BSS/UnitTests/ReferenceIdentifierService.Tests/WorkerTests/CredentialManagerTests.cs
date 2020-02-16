using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Nighthawk.ReferenceIdentifierService.Data;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Workers;

namespace VSS.Nighthawk.ReferenceIdentifierService.Tests.WorkerTests
{
  [TestClass]
  public class CredentialManagerTests
  {
    [TestMethod]
    public void RetrieveByUrl_Test()
    {
      Mock<IStorage> _mockStorage = new Mock<IStorage>();
      CredentialManager manager = new CredentialManager(_mockStorage.Object);
      manager.RetrieveByUrl("Test");
      _mockStorage.Verify(e=>e.FindCredentialsForUrl(It.Is<string>(f=>f=="Test")), Times.Once());

    }
  }
}
