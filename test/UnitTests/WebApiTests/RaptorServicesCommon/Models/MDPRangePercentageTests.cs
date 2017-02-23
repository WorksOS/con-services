
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.ResultHandling;


namespace VSS.Raptor.Service.WebApiTests.Common.Models
{
  [TestClass()]
  public class MDPRangePercentageTests
  {
    [TestMethod()]
    public void CanCreateMdpRangePercentageTest()
    {
      var validator = new DataAnnotationsValidator();
      MDPRangePercentage range = MDPRangePercentage.CreateMdpRangePercentage(35.0, 72.5);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(range, out results));

      //too big max
      range = MDPRangePercentage.CreateMdpRangePercentage(35.0, 1000.0);
      Assert.IsFalse(validator.TryValidate(range, out results));
    }

    [TestMethod()]
    public void ValidateSuccessTest()
    {
      MDPRangePercentage range = MDPRangePercentage.CreateMdpRangePercentage(35.0, 72.5);
      range.Validate();
    }

    [TestMethod()]
    public void ValidateFailTest()
    {
      //min > max
      MDPRangePercentage range = MDPRangePercentage.CreateMdpRangePercentage(85.0, 40.0);
      Assert.ThrowsException<ServiceException>(() => range.Validate());
    }
  }
}
