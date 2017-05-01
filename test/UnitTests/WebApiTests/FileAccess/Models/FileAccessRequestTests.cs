using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.WebApiModels.FileAccess.Models;

namespace VSS.Raptor.Service.WebApiTests.FileAccess.Models
{
  [TestClass()]
  public class FileAccessRequestTests
  {
    [TestMethod()]
    public void CanCreateFileAccessRequestTest()
    {
      var validator = new DataAnnotationsValidator();
      FileDescriptor file = FileDescriptor.CreateFileDescriptor(
        "u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01", "/77561/1158", "Large Sites Road - Trimble Road.ttm");
      string localPath = @"C:\ProductionData\DataModels\1158\Temp\Large Sites Road - Trimble Road.ttm";
      FileAccessRequest request = FileAccessRequest.CreateFileAccessRequest(file, localPath);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(request, out results));

      // Missing file name...
      request = FileAccessRequest.CreateFileAccessRequest(null, localPath);
      Assert.IsFalse(validator.TryValidate(request, out results));

      // Missing local path...
      request = FileAccessRequest.CreateFileAccessRequest(file, null);
      Assert.IsFalse(validator.TryValidate(request, out results));
    }
  }
}
