using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Exceptions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Validation;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApiTests.ProductionData.Models
{
  [TestClass]
  public class PatchRequestTests
  {
    [TestMethod]
    public void CanCreatePatchRequestTest()
    {

      var validator = new DataAnnotationsValidator();
      PatchRequest request = new PatchRequest(
                projectId, null, callId, DisplayMode.Height, null, liftSettings, false, VolumesType.None, 0.0, null, null, null,
                FilterLayerMethod.None, 5, 50);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(request, out results));

      //missing project id
      request = new PatchRequest(
                -1, null, callId, DisplayMode.Height, palettes, liftSettings, false, VolumesType.None, 0.0, null, null, null,
                FilterLayerMethod.None, 5, 50);
      Assert.IsFalse(validator.TryValidate(request, out results));

      //vol no change tolerance out of range
      request = new PatchRequest(
                      projectId, null, callId, DisplayMode.Height, null, liftSettings, false, VolumesType.None, 10.1, null, null, null,
                      FilterLayerMethod.None, 5, 50);
      Assert.IsFalse(validator.TryValidate(request, out results));

      //patch number out of range
      request = new PatchRequest(
                      projectId, null, callId, DisplayMode.Height, null, liftSettings, false, VolumesType.None, 0.0, null, null, null,
                      FilterLayerMethod.None, -1, 50);
      Assert.IsFalse(validator.TryValidate(request, out results));

      //patch size out of range
      request = new PatchRequest(
                      projectId, null, callId, DisplayMode.Height, null, liftSettings, false, VolumesType.None, 0.0, null, null, null,
                      FilterLayerMethod.None, 5, 9999);
      Assert.IsFalse(validator.TryValidate(request, out results));

    }


    [TestMethod]
    public void ValidateFailInvalidPaletteNumberTest()
    {
      //wrong number of palettes for display mode
      PatchRequest request = new PatchRequest(
                projectId, null, callId, DisplayMode.CCVPercent, palettes, liftSettings, false, VolumesType.None, 0.0, null, null, null,
                FilterLayerMethod.None, 5, 50);

      Assert.ThrowsException<ServiceException>(() => request.Validate());
    }

    [TestMethod]
    public void ValidateFailInvalidPaletteOrderTest()
    {
      //palettes out of order
      PatchRequest request = new PatchRequest(
                projectId, null, callId, DisplayMode.Height, invalidPalettes, liftSettings, false, VolumesType.None, 0.0, null, null, null,
                FilterLayerMethod.None, 5, 50);
      Assert.ThrowsException<ServiceException>(() => request.Validate());
    }


    [TestMethod]
    public void ValidateFailMissingDesignTest()
    {
      //missing design (for volumes display mode)
      PatchRequest request = new PatchRequest(
                projectId, null, callId, DisplayMode.CutFill, palettes, liftSettings, false, VolumesType.None, 0.0, null, null, null,
                FilterLayerMethod.None, 5, 50);
      Assert.ThrowsException<ServiceException>(() => request.Validate());
    }


    [TestMethod]
    public void ValidateFailMissingFilterTest()
    {
      //missing filter (for volumes display mode)
      PatchRequest request = new PatchRequest(
                projectId, null, callId, DisplayMode.VolumeCoverage, palettes, liftSettings, false, VolumesType.Between2Filters, 0.0, null, null, null,
                FilterLayerMethod.None, 5, 50);
      Assert.ThrowsException<TwoFiltersRequiredException>(() => request.Validate());
    }

    [TestMethod]
    public void ValidateFailInvalidVolumeTypeTest()
    {
      //Unsupported volume type (for volumes display mode)
      PatchRequest request = new PatchRequest(
                projectId, null, callId, DisplayMode.VolumeCoverage, palettes, liftSettings, false, VolumesType.AboveLevel, 0.0, null, null, null,
                FilterLayerMethod.None, 5, 50);
      Assert.ThrowsException<ServiceException>(() => request.Validate());
    }

    private static readonly LiftThicknessTarget LiftThicknessTarget = new LiftThicknessTarget
    {
      AboveToleranceLiftThickness = (float)0.001,
      BelowToleranceLiftThickness = (float)0.002,
      TargetLiftThickness = (float)0.05
    };


    private const long projectId = 1234;
    private readonly Guid callId = new Guid();
    private readonly LiftBuildSettings liftSettings = new LiftBuildSettings(
      new CCVRangePercentage(80, 110), false, 1.0, 2.0, 0.2f, LiftDetectionType.Automatic, LiftThicknessType.Compacted,
      new MDPRangePercentage(70, 120), false, null, null, null, null, null, null, LiftThicknessTarget, null);

    private List<ColorPalette> palettes = new List<ColorPalette>
                                         {
                                           new ColorPalette(Colors.Red, 0.0),
                                           new ColorPalette(Colors.Lime, 90.0),
                                           new ColorPalette(Colors.Blue, 120.0)
                                         };

    private List<ColorPalette> invalidPalettes = new List<ColorPalette>
                                         {
                                           new ColorPalette(Colors.Lime, 90.0),
                                           new ColorPalette(Colors.Red, 0.0),
                                           new ColorPalette(Colors.Blue, 120.0)
                                         };
    private DesignDescriptor design = new DesignDescriptor(1, null, 0);
  }
}
