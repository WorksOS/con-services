using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Validation;
using VSS.Productivity3D.WebApiModels.Report.Models;

namespace VSS.Productivity3D.WebApiTests.Report.Models
{
  [TestClass]
  public class PassCountSettingsTests
  {
    [TestMethod]
    public void CanCreatePassCountSettingsTest()
    {
      var validator = new DataAnnotationsValidator();
      PassCountSettings settings = PassCountSettings.CreatePassCountSettings(new int[] { 1, 3, 5, 10 });
      Assert.IsTrue(validator.TryValidate(settings, out ICollection<ValidationResult> results));
    }

    [TestMethod]
    public void ValidateSuccessTest()
    {
      PassCountSettings settings = PassCountSettings.CreatePassCountSettings(new int[] { 1, 3, 5, 10 });
      settings.Validate();
    }

    [TestMethod]
    public void ValidateFailLengthTest()
    {
      //empty array
      PassCountSettings settings = PassCountSettings.CreatePassCountSettings(new int[] {});
      Assert.ThrowsException<ServiceException>(() => settings.Validate());
    }

    [TestMethod]
    public void ValidateFailFirstTest()
    {
      //doesn't start at 0
      PassCountSettings settings = PassCountSettings.CreatePassCountSettings(new int[] { 0, 5, 10 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());
    }

    [TestMethod]
    public void ValidateFailOrderTest()
    {
      //pass counts out of order
      PassCountSettings settings = PassCountSettings.CreatePassCountSettings(new int[] { 1, 10, 5, 12, 3 });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());
    }


    [TestMethod]
    public void ValidateFailRangeTest()
    {
      //pass count value > max
      PassCountSettings settings = PassCountSettings.CreatePassCountSettings(new int[] { 1, 2, int.MaxValue });
      Assert.ThrowsException<ServiceException>(() => settings.Validate());
    }


  }
}
