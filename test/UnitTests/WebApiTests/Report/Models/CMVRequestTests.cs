using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Validation;
using VSS.Productivity3D.WebApiModels.Report.Models;

namespace VSS.Productivity3D.WebApiTests.Report.Models
{
  [TestClass]
  public class CMVRequestTests
  {
    [TestMethod]
    public void CanCreateCMVRequestTest()
    {
      //******************* isCustomCMVTargets = false **************************
      var validator = new DataAnnotationsValidator();
      CMVRequest request = CMVRequest.CreateCMVRequest(projectId, callId, cmvSettings, liftSettings, null, 0, null, null, null);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(request, out results));

      //missing project id
      request = CMVRequest.CreateCMVRequest(-1, callId, cmvSettings, liftSettings, null, 0, null, null, null);
      Assert.IsFalse(validator.TryValidate(request, out results));

      //missing CMV settings
      request = CMVRequest.CreateCMVRequest(projectId, callId, null, liftSettings, null, 0, null, null, null);
      Assert.IsFalse(validator.TryValidate(request, out results));

      //******************* isCustomCMVTargets = true ***************************
      request = CMVRequest.CreateCMVRequest(projectId, callId, cmvSettingsEx, liftSettings, null, 0, null, null, null, true);
      Assert.IsTrue(validator.TryValidate(request, out results));

      //missing project id
      request = CMVRequest.CreateCMVRequest(-1, callId, cmvSettingsEx, liftSettings, null, 0, null, null, null, true);
      Assert.IsFalse(validator.TryValidate(request, out results));

      //missing CMV settings
      request = CMVRequest.CreateCMVRequest(projectId, callId, null, liftSettings, null, 0, null, null, null, true);
      Assert.IsFalse(validator.TryValidate(request, out results));
    }

    [TestMethod]
    public void ValidateSuccessTest()
    {
      //******************* isCustomCMVTargets = false **************************
      CMVRequest request = CMVRequest.CreateCMVRequest(projectId, callId, cmvSettings, liftSettings, null, 0, null, null, null);
      request.Validate();

      //******************* isCustomCMVTargets = true **************************
      request = CMVRequest.CreateCMVRequest(projectId, callId, cmvSettingsEx, liftSettings, null, 0, null, null, null, true);
      request.Validate();
    }

    [TestMethod]
    public void ValidateFailInvalidOverrideDatesTest()
    {
      // override startUTC > override end UTC
      //******************* isCustomCMVTargets = false **************************
      CMVRequest request = CMVRequest.CreateCMVRequest(projectId, callId, cmvSettings, liftSettings, null, 0, new DateTime(2014, 1, 31), new DateTime(2014, 1, 1), null);
      Assert.ThrowsException<ServiceException>(() => request.Validate());

      //******************* isCustomCMVTargets = true **************************
      request = CMVRequest.CreateCMVRequest(projectId, callId, cmvSettingsEx, liftSettings, null, 0, new DateTime(2014, 1, 31), new DateTime(2014, 1, 1), null, true);
      Assert.ThrowsException<ServiceException>(() => request.Validate());
    }

    [TestMethod]
    public void ValidateFailMissingOverrideDatesTest()
    {
      //missing override end UTC
      //******************* isCustomCMVTargets = false **************************
      CMVRequest request = CMVRequest.CreateCMVRequest(projectId, callId, cmvSettings, liftSettings, null, 0, new DateTime(2014, 1, 1), null, null);
      Assert.ThrowsException<ServiceException>(() => request.Validate());

      //******************* isCustomCMVTargets = true **************************
      request = CMVRequest.CreateCMVRequest(projectId, callId, cmvSettingsEx, liftSettings, null, 0, new DateTime(2014, 1, 1), null, null, true);
      Assert.ThrowsException<ServiceException>(() => request.Validate());
    }

    private static readonly LiftThicknessTarget LiftThicknessTarget = new LiftThicknessTarget
    {
      AboveToleranceLiftThickness = (float)0.001,
      BelowToleranceLiftThickness = (float)0.002,
      TargetLiftThickness = (float)0.05
    };

    private long projectId = 1234;
    private Guid callId = new Guid();
    private CMVSettings cmvSettings = CMVSettings.CreateCMVSettings(800, 1200, 110.0, 700, 85.0, false);
    private CMVSettingsEx cmvSettingsEx = CMVSettingsEx.CreateCMVSettingsEx(800, 1200, 110.0, 700, 85.0, false, new []{ 0, 40, 80, 120, 150 });
    private LiftBuildSettings liftSettings = LiftBuildSettings.CreateLiftBuildSettings(
      CCVRangePercentage.CreateCcvRangePercentage(80, 110), false, 1.0, 2.0, 0.2f, LiftDetectionType.Automatic, LiftThicknessType.Compacted,
      MDPRangePercentage.CreateMdpRangePercentage(70, 120), false, null, null, null, null, null, null, LiftThicknessTarget, null);
  }
}
