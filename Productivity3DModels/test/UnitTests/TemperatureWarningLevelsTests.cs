using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Validation;

namespace VSS.Productivity3D.Models.UnitTests
{
  [TestClass]
  public class TemperatureWarningLevelsTests
  {
    [TestMethod]
    public void CanCreateTemperatureWarningLevelsTest()
    {
      var validator = new DataAnnotationsValidator();
      TemperatureWarningLevels range = TemperatureWarningLevels.CreateTemperatureWarningLevels(300, 700);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(range, out results));

      //too big max
      range = TemperatureWarningLevels.CreateTemperatureWarningLevels(0, 10000);
      Assert.IsFalse(validator.TryValidate(range, out results));
    }

    [TestMethod]
    public void ValidateSuccessTest()
    {
      //min > max
      TemperatureWarningLevels range = TemperatureWarningLevels.CreateTemperatureWarningLevels(300, 700);
      range.Validate();
    }

    [TestMethod]
    public void ValidateFailTest()
    {
      TemperatureWarningLevels range = TemperatureWarningLevels.CreateTemperatureWarningLevels(700, 300);
      Assert.ThrowsException<ServiceException>(() => range.Validate());
    }
  }
}
