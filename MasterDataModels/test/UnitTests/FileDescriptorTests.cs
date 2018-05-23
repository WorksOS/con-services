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
    public void CanValidateFileDescriptorWithCustomerAndProjectID()
    {
      var desc = FileDescriptor.CreateFileDescriptor("1", "132465", "55644", "FILE.FILE");
      desc.Validate();
      Assert.AreEqual("/132465/55644", desc.path);
      Assert.AreEqual("FILE.FILE", desc.fileName);
      Assert.AreEqual("1", desc.filespaceId);
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
