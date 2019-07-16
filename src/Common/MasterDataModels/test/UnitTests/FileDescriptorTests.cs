using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using Xunit;

namespace VSS.MasterData.Models.UnitTests
{
  public class FileDescriptorTests
  {
    [Fact]
    public void CanCreateFileDescriptor()
    {
      var desc = FileDescriptor.CreateFileDescriptor("1", "2.txt", "3");
      Assert.NotNull(desc);
    }

    [Fact]
    public void CanValidateFileDescriptor()
    {
      var desc = FileDescriptor.CreateFileDescriptor("1", "2.txt", "3");
      desc.Validate();
      Assert.False(false);
    }

    [Fact]
    public void CanValidateFileDescriptorWithCustomerAndProjectID()
    {
      var desc = FileDescriptor.CreateFileDescriptor("1", "132465", "55644", "FILE.FILE");
      desc.Validate();
      Assert.Equal("/132465/55644", desc.Path);
      Assert.Equal("FILE.FILE", desc.FileName);
      Assert.Equal("1", desc.FilespaceId);
    }

    [Fact]
    public void CanValidateInvalidFileDescriptor()
    {
      Assert.Throws<ServiceException>(() => FileDescriptor.CreateFileDescriptor("1", null, "3").Validate());
    }
  }
}
