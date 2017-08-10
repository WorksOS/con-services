using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.Models;

namespace VSS.Productivity3D.WebApiTests.Report.Models
{
  [TestClass]
  public class CMVSettingsTests
  {
    [TestMethod]
    public void CanCreateCMVSettingsTest()
    {
      var validator = new DataAnnotationsValidator();
      var settings = CMVSettings.CreateCMVSettings(800, 1200, 110.0, 700, 85.0, false);

      Assert.IsTrue(validator.TryValidate(settings, out ICollection<ValidationResult> results));

      //Max out of range
      settings = CMVSettings.CreateCMVSettings(800, -1, 110.0, 700, 85.0, false);
      Assert.IsFalse(validator.TryValidate(settings, out results));

      //Min percent out of range
      settings = CMVSettings.CreateCMVSettings(800, 1200, 110.0, 700, 300.0, false);
      Assert.IsFalse(validator.TryValidate(settings, out results));
    }

    [TestMethod]
    public void Validate_Should_succeed_When_inputs_are_valid_and_override_is_false()
    {
      CMVSettings.CreateCMVSettings(800, 1200, 110.0, 700, 85.0, false).Validate();
    }

    [TestMethod]
    [DataRow((short)-1, (short)1, 1.0, (short)1, 1.0)]
    [DataRow((short)0, (short)1, 1.0, (short)1, 1.0)]
    public void Validate_Should_return_When_cmvTarget_fails_validation_and_override_is_false(short cmvTarget, short minCmv, double maxCmv, short minCmvPercent, double maxCmvPercent)
    {
      CMVSettings.CreateCMVSettings(cmvTarget, minCmv, maxCmv, minCmvPercent, maxCmvPercent, false).Validate();
    }

    [TestMethod]
    public void Validate_Should_succeed_When_inputs_are_valid_and_override_is_true()
    {
      CMVSettings.CreateCMVSettings(1, 1, 1.0, 1, 1.0, true).Validate();
    }

    [TestMethod]
    [DataRow((short)1, (short)0, 0.0, (short)0, 0.0)]
    [DataRow((short)1, (short)1, 1.0, (short)0, 0.0)]
    [DataRow((short)0, (short)0, 0.0, (short)1, 1.0)]
    [DataRow((short)-1, (short)0, 0.0, (short)1, 1.0)]
    [DataRow((short)-1, (short)1, 1.0, (short)1, 1.0)]
    [DataRow((short)1, (short)0, 0.0, (short)1, 1.0)]
    [DataRow((short)1, (short)1, 1.0, (short)2, 1.0)]
    public void Validate_Should_throw_When_input_settings_fail_to_validate_and_override_is_true(short cmvTarget, short minCmv, double maxCmv, short minCmvPercent, double maxCmvPercent)
    {
      Assert.ThrowsException<ServiceException>(
        () => CMVSettings.CreateCMVSettings(cmvTarget, minCmv, maxCmv, minCmvPercent, maxCmvPercent, true)
        .Validate());
    }

    [TestMethod]
    [DataRow((short)1, (short)0, 0.0, (short)1, 0.0)]
    [DataRow((short)1, (short)0, 0.0, (short)2, 1.0)]
    public void ValidateRange_Should_throw_When_minCMVPercent_greater_than_maxCMVPercent(short cmvTarget, short minCmv, double maxCmv, short minCmvPercent, double maxCmvPercent)
    {
      Assert.ThrowsException<ServiceException>(
        () => CMVSettings.CreateCMVSettings(cmvTarget, minCmv, maxCmv, minCmvPercent, maxCmvPercent, true)
          .Validate());
    }
  }
}