using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Models.UnitTests
{

  [TestClass]
  public class FileDescriptorTests
  {

    [TestMethod]
    public void CanCreateFileDescriptor()
    {
      var desc = FileDescriptor.CreateFileDescriptor("1", "2.txt", "3");
      Assert.IsNotNull(desc);
    }

    [TestMethod]
    public void CanValidateFileDescriptor()
    {
      var desc = FileDescriptor.CreateFileDescriptor("1", "2.txt", "3");
      desc.Validate();
      Assert.IsFalse(false);
    }

    [TestMethod]
    [ExpectedException(typeof(ServiceException))]
    public void CanValidateInvalidFileDescriptor()
    {
      var desc = FileDescriptor.CreateFileDescriptor("1", null, "3");
      desc.Validate();
    }

  }
}
