
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.WebApiModels.Report.Models;

namespace VSS.Raptor.Service.WebApiTests.Report.Models
{
  [TestClass()]
  public class CMVSettingsTests
  {
    [TestMethod()]
    public void CanCreateCMVSettingsTest()
    {
      var validator = new DataAnnotationsValidator();
      CMVSettings settings = CMVSettings.CreateCMVSettings(800, 1200, 110.0, 700, 85.0, false);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(settings, out results));

      //Max out of range
      settings = CMVSettings.CreateCMVSettings(800, -1, 110.0, 700, 85.0, false);
      Assert.IsFalse(validator.TryValidate(settings, out results));

      //Min percent out of range
      settings = CMVSettings.CreateCMVSettings(800, 1200, 110.0, 700, 300.0, false);
      Assert.IsFalse(validator.TryValidate(settings, out results));
    }

    [TestMethod()]
    public void ValidateSuccessTest()
    {
      CMVSettings settings = CMVSettings.CreateCMVSettings(800, 1200, 110.0, 700, 85.0, false);
      settings.Validate();
    }

    [TestMethod()]
    public void ValidateFailRangeTest()
    {
      //min > max
      CMVSettings settings = CMVSettings.CreateCMVSettings(800, 700, 110.0, 1200, 85.0, false);
      Assert.ThrowsException<ServiceException>(() => settings.Validate());
    }
  }
}
