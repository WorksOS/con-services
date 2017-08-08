using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.Models;

namespace VSS.Productivity3D.WebApiTests.Report.Models
{
  [TestClass]
  public class CMVRequestTests
  {
    [TestMethod]
    public void CanCreateCMVRequestTest()
    {
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
    }

    [TestMethod]
    public void ValidateSuccessTest()
    {
      CMVRequest request = CMVRequest.CreateCMVRequest(projectId, callId, cmvSettings, liftSettings, null, 0, null, null, null);
      request.Validate();
    }

    [TestMethod]
    public void ValidateFailInvalidOverrideDatesTest()
    {
      //override startUTC > override end UTC
      CMVRequest request = CMVRequest.CreateCMVRequest(projectId, callId, cmvSettings, liftSettings, null, 0, new DateTime(2014, 1, 31), new DateTime(2014, 1, 1), null);
      Assert.ThrowsException<ServiceException>(() => request.Validate());
    }

    [TestMethod]
    public void ValidateFailMissingOverrideDatesTest()
    {
      //missing override end UTC
      CMVRequest request = CMVRequest.CreateCMVRequest(projectId, callId, cmvSettings, liftSettings, null, 0, new DateTime(2014, 1, 1), null, null);
      Assert.ThrowsException<ServiceException>(() => request.Validate());
    }

    private long projectId = 1234;
    private Guid callId = new Guid();
    private CMVSettings cmvSettings = CMVSettings.CreateCMVSettings(800, 1200, 110.0, 700, 85.0, false);
    private LiftBuildSettings liftSettings = LiftBuildSettings.CreateLiftBuildSettings(
      CCVRangePercentage.CreateCcvRangePercentage(80, 110), false, 1.0, 2.0, 0.2f, LiftDetectionType.Automatic, LiftThicknessType.Compacted,
      MDPRangePercentage.CreateMdpRangePercentage(70, 120), false, null, null, null, null, null, null, LiftThicknessTarget.HelpSample, null);
  }
}
