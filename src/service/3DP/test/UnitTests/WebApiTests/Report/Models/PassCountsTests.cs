using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Validation;
using VSS.Productivity3D.WebApi.Models.Report.Models;

namespace VSS.Productivity3D.WebApiTests.Report.Models
{
  [TestClass]
  public class PassCountsTests
  {
    private long projectId = 1234;
    private readonly PassCountSettings passCountSettings = PassCountSettings.CreatePassCountSettings(new[] { 1, 3, 5, 10 });
    private readonly LiftBuildSettings liftSettings = new LiftBuildSettings(
      new CCVRangePercentage(80, 110), false, 1.0, 2.0, 0.2f, LiftDetectionType.Automatic, LiftThicknessType.Compacted,
      new MDPRangePercentage(70, 120), false, null, null, null, null, null, null, LiftThicknessTarget, null);

    [TestMethod]
    public void CanCreatePassCountsTest()
    {
      var validator = new DataAnnotationsValidator();
      var passCounts = new PassCounts(projectId, null, passCountSettings, liftSettings, null, 0, null, null, null);

      Assert.IsTrue(validator.TryValidate(passCounts, out var results));

      Assert.IsNotNull(results);
      Assert.IsTrue(results.Count == 0);
    }

    [TestMethod]
    public void CanCreatePassCountsTestMissingProjectId()
    {
      var validator = new DataAnnotationsValidator();
      var passCounts = new PassCounts(0, null, passCountSettings, liftSettings, null, 0, null, null, null);

      Assert.IsFalse(validator.TryValidate(passCounts, out var results));

      Assert.IsNotNull(results);
      Assert.AreEqual("Invalid project ID", results.First().ErrorMessage);
    }

    [TestMethod]
    public void ValidateSuccessTest()
    {
      PassCounts passCounts = new PassCounts(projectId, null, passCountSettings, liftSettings, null, 0, null, null, null);
      passCounts.Validate();
    }


    [TestMethod]
    public void ValidateFailInvalidOverrideDatesTest()
    {
      //override startUTC > override end UTC
      PassCounts passCounts = new PassCounts(projectId, null, passCountSettings, liftSettings, null, 0, new DateTime(2014, 1, 31), new DateTime(2014, 1, 1), null);
      Assert.ThrowsException<ServiceException>(() => passCounts.Validate());
    }

    [TestMethod]
    public void ValidateFailMissingOverrideDatesTest()
    {
      //missing override end UTC
      PassCounts passCounts = new PassCounts(projectId, null, passCountSettings, liftSettings, null, 0, new DateTime(2014, 1, 1), null, null);
      Assert.ThrowsException<ServiceException>(() => passCounts.Validate());
    }

    private static readonly LiftThicknessTarget LiftThicknessTarget = new LiftThicknessTarget
    {
      AboveToleranceLiftThickness = (float)0.001,
      BelowToleranceLiftThickness = (float)0.002,
      TargetLiftThickness = (float)0.05
    };
  }
}
