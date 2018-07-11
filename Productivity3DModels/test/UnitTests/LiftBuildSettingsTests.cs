using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Validation;

namespace VSS.Productivity3D.Models.UnitTests
{
  [TestClass]
  public class LiftBuildSettingsTests
  {
    private readonly LiftThicknessTarget _liftThicknessTarget = new LiftThicknessTarget
    {
      AboveToleranceLiftThickness = (float)0.001,
      BelowToleranceLiftThickness = (float)0.002,
      TargetLiftThickness = (float)0.05
    };

    [TestMethod]
    public void CanCreateLiftBuildSettingsTest()
    {
      var validator = new DataAnnotationsValidator();
      LiftBuildSettings settings = LiftBuildSettings.CreateLiftBuildSettings(
        CCVRangePercentage.CreateCcvRangePercentage(30.0, 70.0), false, 0.0, 0.0, 0.2f, LiftDetectionType.Automatic,
        LiftThicknessType.Compacted, MDPRangePercentage.CreateMdpRangePercentage(35.0, 75.0),
        false, null, null, null, null, null, null, _liftThicknessTarget, null);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(settings, out results));

      //no CCV range
      /* settings = LiftBuildSettings.CreateLiftBuildSettings(
         null, false, 2.3, 0.0, 0.2f, LiftDetectionType.Automatic,
         LiftThicknessType.Compacted, MDPRangePercentage.CreateMdpRangePercentage(35.0, 75.0),
        false, null, null, null, null, null, null, liftThicknessTarget);
       Assert.IsFalse(validator.TryValidate(settings, out results), "CCV range required failed");

       //no MDP range
       settings = LiftBuildSettings.CreateLiftBuildSettings(
         CCVRangePercentage.CreateCcvRangePercentage(30.0, 70.0), false, 0.0, 0.0, 0.2f, LiftDetectionType.Automatic,
         LiftThicknessType.Compacted, null, false, null, null, null, null, null, null, liftThicknessTarget);
       Assert.IsFalse(validator.TryValidate(settings, out results), "MDP range required failed");*/

      //dead band lower boundary out of range
      settings = LiftBuildSettings.CreateLiftBuildSettings(
         CCVRangePercentage.CreateCcvRangePercentage(30.0, 70.0), false, 2.3, 0.0, 0.2f, LiftDetectionType.Automatic,
         LiftThicknessType.Compacted, MDPRangePercentage.CreateMdpRangePercentage(35.0, 75.0),
        false, null, null, null, null, null, null, _liftThicknessTarget, null);
      Assert.IsFalse(validator.TryValidate(settings, out results), "dead band lower validate failed");

      //dead band upper boundary out of range
      settings = LiftBuildSettings.CreateLiftBuildSettings(
         CCVRangePercentage.CreateCcvRangePercentage(30.0, 70.0), false, 0.0, 1.5, 0.2f, LiftDetectionType.Automatic,
         LiftThicknessType.Compacted, MDPRangePercentage.CreateMdpRangePercentage(35.0, 75.0),
        false, null, null, null, null, null, null, _liftThicknessTarget, null);
      Assert.IsFalse(validator.TryValidate(settings, out results), "dead band upper validate failed");

      //first pass thickness out of range
      settings = LiftBuildSettings.CreateLiftBuildSettings(
         CCVRangePercentage.CreateCcvRangePercentage(30.0, 70.0), false, 0.0, 0.0, -0.2f, LiftDetectionType.Automatic,
         LiftThicknessType.Compacted, MDPRangePercentage.CreateMdpRangePercentage(35.0, 75.0),
        false, null, null, null, null, null, null, _liftThicknessTarget, null);
      Assert.IsFalse(validator.TryValidate(settings, out results), "first pass thickness validate failed");

      //overriding lift thickness out of range
      settings = LiftBuildSettings.CreateLiftBuildSettings(
         CCVRangePercentage.CreateCcvRangePercentage(30.0, 70.0), false, 0.0, 0.0, 0.2f, LiftDetectionType.Automatic,
         LiftThicknessType.Compacted, MDPRangePercentage.CreateMdpRangePercentage(35.0, 75.0),
        false, 104.1f, null, null, null, null, null, _liftThicknessTarget, null);
      Assert.IsFalse(validator.TryValidate(settings, out results), "overriding lift thickness validate failed");

      //overriding CCV out of range
      settings = LiftBuildSettings.CreateLiftBuildSettings(
         CCVRangePercentage.CreateCcvRangePercentage(30.0, 70.0), false, 0.0, 0.0, 0.2f, LiftDetectionType.Automatic,
         LiftThicknessType.Compacted, MDPRangePercentage.CreateMdpRangePercentage(35.0, 75.0),
        false, null, 1001, null, null, null, null, _liftThicknessTarget, null);
      Assert.IsFalse(validator.TryValidate(settings, out results), "overriding CCV validate failed");

      //overriding MDP out of range
      settings = LiftBuildSettings.CreateLiftBuildSettings(
         CCVRangePercentage.CreateCcvRangePercentage(30.0, 70.0), false, 0.0, 0.0, 0.2f, LiftDetectionType.Automatic,
         LiftThicknessType.Compacted, MDPRangePercentage.CreateMdpRangePercentage(35.0, 75.0),
        false, null, null, 11230, null, null, null, _liftThicknessTarget, null);
      Assert.IsFalse(validator.TryValidate(settings, out results), "overriding MDP validate failed");
    }

    [TestMethod]
    public void ValidateSuccessTest()
    {
      LiftBuildSettings settings = LiftBuildSettings.CreateLiftBuildSettings(
              CCVRangePercentage.CreateCcvRangePercentage(30.0, 70.0), false, 0.0, 0.0, 0.2f, LiftDetectionType.Automatic,
              LiftThicknessType.Compacted, MDPRangePercentage.CreateMdpRangePercentage(35.0, 75.0), false, 0.0f, 0, 0, null, null, null, _liftThicknessTarget, null);
      settings.Validate();

      settings = LiftBuildSettings.CreateLiftBuildSettings(
              CCVRangePercentage.CreateCcvRangePercentage(30.0, 70.0), false, 0.0, 0.0, 0.2f, LiftDetectionType.Automatic,
              LiftThicknessType.Compacted, MDPRangePercentage.CreateMdpRangePercentage(35.0, 75.0),
              true, 2.0f, 140, 125, TargetPassCountRange.CreateTargetPassCountRange(3, 3),
              TemperatureWarningLevels.CreateTemperatureWarningLevels(300, 800), false, _liftThicknessTarget, null);
      settings.Validate();
    }

    [TestMethod]
    public void ValidateFailTest()
    {
      //min temp > max temp
      LiftBuildSettings settings = LiftBuildSettings.CreateLiftBuildSettings(
              CCVRangePercentage.CreateCcvRangePercentage(30.0, 70.0), false, 0.0, 0.0, 0.2f, LiftDetectionType.Automatic,
              LiftThicknessType.Compacted, MDPRangePercentage.CreateMdpRangePercentage(35.0, 75.0), false, 0.0f, 0, 0, null,
              TemperatureWarningLevels.CreateTemperatureWarningLevels(700, 500), false, _liftThicknessTarget, null);
      Assert.ThrowsException<ServiceException>(() => settings.Validate());
    }

    [TestMethod]
    public void ValidatePassCountTargetRangeFailTest()
    {
      //min pass count target > max pass count target
      LiftBuildSettings settings = LiftBuildSettings.CreateLiftBuildSettings(
              CCVRangePercentage.CreateCcvRangePercentage(30.0, 70.0), false, 0.0, 0.0, 0.2f, LiftDetectionType.Automatic,
              LiftThicknessType.Compacted, MDPRangePercentage.CreateMdpRangePercentage(35.0, 75.0), false, 0.0f, 0, 0,
              TargetPassCountRange.CreateTargetPassCountRange(10, 1), TemperatureWarningLevels.CreateTemperatureWarningLevels(500, 700), false, _liftThicknessTarget, null);
      Assert.ThrowsException<ServiceException>(() => settings.Validate());
    }

  }
}