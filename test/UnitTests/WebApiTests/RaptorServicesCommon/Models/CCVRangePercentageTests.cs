using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.RaptorServicesCommon.Models
{
  [TestClass]
  public class CCVRangePercentageTests
  {
    [TestMethod]
    public void CanCreateCCVRangePercentageTest()
    {
      var validator = new DataAnnotationsValidator();
      CCVRangePercentage range = CCVRangePercentage.CreateCcvRangePercentage(35.0, 72.5);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(range, out results));

      //too big max
      range = CCVRangePercentage.CreateCcvRangePercentage(35.0, 1000.0);
      Assert.IsFalse(validator.TryValidate(range, out results));
    }

    [TestMethod]
    public void ValidateSuccessTest()
    {
      CCVRangePercentage range = CCVRangePercentage.CreateCcvRangePercentage(35.0, 72.5);
      range.Validate();
    }

    [TestMethod]
    public void ValidateFailTest()
    {
      //min > max
      CCVRangePercentage range = CCVRangePercentage.CreateCcvRangePercentage(85.0, 40.0);
      Assert.ThrowsException<ServiceException>(() => range.Validate());
    }
  }
}
