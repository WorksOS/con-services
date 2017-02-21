using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.ResultHandling;

namespace VSS.Raptor.Service.WebApiTests.Common.Models
{
  [TestClass()]
  public class TargetPassCountRangeTest
  {
    [TestMethod()]
    public void CanCreateTargetPassCountRangeTest()
    {
      var validator = new DataAnnotationsValidator();
      TargetPassCountRange range = TargetPassCountRange.CreateTargetPassCountRange(1, 10);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(range, out results));

      //too small min
      range = TargetPassCountRange.CreateTargetPassCountRange(0, 100);
      Assert.IsFalse(validator.TryValidate(range, out results));
    }

    [TestMethod()]
    public void ValidateSuccessTest()
    {
      TargetPassCountRange range = TargetPassCountRange.CreateTargetPassCountRange(5, 200);
      range.Validate();
    }

    [TestMethod()]
    [ExpectedException(typeof(ServiceException))]
    public void ValidateFailTest()
    {
      //min > max
      TargetPassCountRange range = TargetPassCountRange.CreateTargetPassCountRange(100, 2);
      range.Validate();
    }
  }
}
