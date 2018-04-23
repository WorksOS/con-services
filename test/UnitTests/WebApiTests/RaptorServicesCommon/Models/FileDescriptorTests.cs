using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.RaptorServicesCommon.Models
{
  [TestClass]
  public class FileDescriptorTests
  {
    [TestMethod]
    public void CanCreateFileDescriptorTest()
    {
      string bigString = new string('A', 10000);

      var validator = new DataAnnotationsValidator();
      FileDescriptor file = FileDescriptor.CreateFileDescriptor("u72003136-d859-4be8-86de-c559c841bf10",
          "BC Data/Sites/Integration10/Designs", "Cycleway.ttm");
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(file, out results));

      //Note: file name is string.Empty due to the ValidFilename attribute otherwise get a null reference exception
      file = FileDescriptor.CreateFileDescriptor(null, null, string.Empty);
      Assert.IsFalse(validator.TryValidate(file, out results), "empty file descriptor failed");

      //too big path
      file = FileDescriptor.CreateFileDescriptor("u72003136-d859-4be8-86de-c559c841bf10",
                bigString, "Cycleway.ttm");
      Assert.IsFalse(validator.TryValidate(file, out results), " too big path failed");

      //too big file name
      file = FileDescriptor.CreateFileDescriptor("u72003136-d859-4be8-86de-c559c841bf10",
          "BC Data/Sites/Integration10/Designs", bigString);
      Assert.IsFalse(validator.TryValidate(file, out results), "too big file name failed");
    }

    [TestMethod]
    public void ValidateSuccessTest()
    {
      FileDescriptor file = FileDescriptor.CreateFileDescriptor("u72003136-d859-4be8-86de-c559c841bf10",
          "BC Data/Sites/Integration10/Designs", "Cycleway.ttm"); 
      file.Validate();
    }

    [TestMethod]
    public void ValidateFailEmptyTest()
    {
      //empty file descriptor
      FileDescriptor file = FileDescriptor.CreateFileDescriptor(
          string.Empty, string.Empty, string.Empty);
      Assert.ThrowsException<ServiceException>(() => file.Validate());
    }


  }
}
