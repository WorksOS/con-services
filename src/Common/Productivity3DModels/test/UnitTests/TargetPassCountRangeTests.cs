using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Validation;

namespace VSS.Productivity3D.Models.UnitTests
{
  [TestClass]
  public class TargetPassCountRangeTests
  {
    [TestMethod]
    public void CanCreateTargetPassCountRangeTest()
    {
      var validator = new DataAnnotationsValidator();
      TargetPassCountRange range = new TargetPassCountRange(1, 10);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(range, out results));

      //too small min
      range = new TargetPassCountRange(0, 100);
      Assert.IsFalse(validator.TryValidate(range, out results));
    }

    [TestMethod]
    public void ValidateSuccessTest()
    {
      TargetPassCountRange range = new TargetPassCountRange(5, 200);
      range.Validate();
    }

    [TestMethod]
    public void ValidateFailTest()
    {
      //min > max
      TargetPassCountRange range = new TargetPassCountRange(100, 2);
      Assert.ThrowsException<ServiceException>(() => range.Validate());
    }
  }
}
