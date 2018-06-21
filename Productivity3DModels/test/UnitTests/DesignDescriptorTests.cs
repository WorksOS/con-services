using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.UnitTests.ResultsHandling;

namespace VSS.Productivity3D.Models.UnitTests
{
  [TestClass]
  public class DesignDescriptorTests
  {

    [TestMethod]
    public void CanCreateDesignDescriptorTest()
    {
      var validator = new DataAnnotationsValidator();
      DesignDescriptor design = DesignDescriptor.CreateDesignDescriptor(1234, null, 0);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(design, out results));
    }

    [TestMethod]
    public void ValidateSuccessTest()
    {
      DesignDescriptor design = DesignDescriptor.CreateDesignDescriptor(1234, null, 0);
      design.Validate();

      design = DesignDescriptor.CreateDesignDescriptor(0,
        FileDescriptor.CreateFileDescriptor("u72003136-d859-4be8-86de-c559c841bf10",
          "BC Data/Sites/Integration10/Designs", "Cycleway.ttm"), 0);
      design.Validate();
    }

    [TestMethod]
    public void ValidateFailEmptyTest()
    {
      //empty design descriptor
      DesignDescriptor design = DesignDescriptor.CreateDesignDescriptor(0, null, 0);
      Assert.ThrowsException<ServiceException>(() => design.Validate());
    }
  }
}
