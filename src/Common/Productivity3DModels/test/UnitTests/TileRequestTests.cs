using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Converters;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Exceptions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Validation;

namespace VSS.Productivity3D.Models.UnitTests
{
  public class TileRequestTests
  {
    BoundingBox2DLatLon boundingBox2DLatLon = new BoundingBox2DLatLon(
      35.109149 * Coordinates.DEGREES_TO_RADIANS,
      -106.604076 * Coordinates.DEGREES_TO_RADIANS,
      35.39012 * Coordinates.DEGREES_TO_RADIANS,
      -105.234 * Coordinates.DEGREES_TO_RADIANS);

    [TestMethod]
    public void CanCreateTileRequestTest()
    {

      var validator = new DataAnnotationsValidator();
      TileRequest request = new TileRequest(
        projectId, null, callId, DisplayMode.Height, null, liftSettings, VolumesType.None, 0.0, null, null, 0, null, 0,
        FilterLayerMethod.None, boundingBox2DLatLon, null, 256, 256);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(request, out results));

      //missing project id
      request = new TileRequest(-1, null, callId, DisplayMode.Height, null, liftSettings, VolumesType.None, 0.0, null, null, 0, null, 0,
        FilterLayerMethod.None, boundingBox2DLatLon, null, 256, 256);
      Assert.IsFalse(validator.TryValidate(request, out results));

      //vol no change tolerance out of range
      request = new TileRequest(projectId, null, callId, DisplayMode.Height, null, liftSettings, VolumesType.None, 10.1, null, null, 0, null, 0,
        FilterLayerMethod.None, boundingBox2DLatLon, null, 256, 256);
      Assert.IsFalse(validator.TryValidate(request, out results));

      //width out of range
      request = new TileRequest(projectId, null, callId, DisplayMode.Height, null, liftSettings, VolumesType.None, 0.0, null, null, 0, null, 0,
        FilterLayerMethod.None, boundingBox2DLatLon, null, 16, 256);
      Assert.IsFalse(validator.TryValidate(request, out results));

      //height out of range
      request = new TileRequest(projectId, null, callId, DisplayMode.Height, null, liftSettings, VolumesType.None, 0.0, null, null, 0, null, 0,
        FilterLayerMethod.None, boundingBox2DLatLon, null, 256, 16000);
      Assert.IsFalse(validator.TryValidate(request, out results));

    }

    [TestMethod]
    public void ValidateSuccessTest()
    {
      TileRequest request = new TileRequest(projectId, null, callId, DisplayMode.Height, null, liftSettings, VolumesType.None, 0.0, null, null, 0, null, 0, FilterLayerMethod.None, boundingBox2DLatLon, null, 256, 256);
      request.Validate();

    }


    [TestMethod]
    public void ValidateFailInvalidPaletteNumberTest()
    {
      //wrong number of palettes for display mode
      TileRequest request = new TileRequest(projectId, null, callId, DisplayMode.CCVPercent, palettes, liftSettings, VolumesType.None, 0.0, null, null, 0, null, 0, FilterLayerMethod.None, boundingBox2DLatLon, null, 256, 256);
      Assert.ThrowsException<ServiceException>(() => request.Validate());
    }

    [TestMethod]
    public void ValidateFailInvalidPaletteOrderTest()
    {
      //palettes out of order
      TileRequest request = new TileRequest(projectId, null, callId, DisplayMode.Height, invalidPalettes, liftSettings, VolumesType.None, 0.0, null, null, 0, null, 0, FilterLayerMethod.None, boundingBox2DLatLon, null, 256, 256);
      Assert.ThrowsException<ServiceException>(() => request.Validate());
    }


    [TestMethod]
    public void ValidateFailMissingDesignTest()
    {
      //missing design (for cutfill display mode)
      TileRequest request = new TileRequest(projectId, null, callId, DisplayMode.CutFill, null, liftSettings,
        VolumesType.BetweenDesignAndFilter, 0.0, null, null, 0, null, 0,
        FilterLayerMethod.None, boundingBox2DLatLon, null, 256, 256);
      Assert.ThrowsException<MissingDesignDescriptorException>(() => request.Validate());
    }


    [TestMethod]
    public void ValidateFailMissingFilterTest()
    {
      //missing filter (for volumes display mode)
      TileRequest request = new TileRequest(projectId, null, callId, DisplayMode.VolumeCoverage, null, liftSettings, VolumesType.Between2Filters, 0.0, null, null, 0, null, 0, FilterLayerMethod.None, boundingBox2DLatLon, null, 256, 256);
      Assert.ThrowsException<TwoFiltersRequiredException>(() => request.Validate());
    }

    [TestMethod]
    public void ValidateFailInvalidVolumeTypeTest()
    {
      //Unsupported volume type (for volumes display mode)
      TileRequest request = new TileRequest(projectId, null, callId, DisplayMode.VolumeCoverage, null, liftSettings, VolumesType.AboveLevel, 0.0, null, null, 0, null, 0, FilterLayerMethod.None, boundingBox2DLatLon, null, 256, 256);
      Assert.ThrowsException<ServiceException>(() => request.Validate());
    }

    [TestMethod]
    public void ValidateFailMissingBoundingBoxTest()
    {
      //missing bounding box
      TileRequest request = new TileRequest(projectId, null, callId, DisplayMode.Height, null, liftSettings, VolumesType.None, 0.0, null, null, 0, null, 0, FilterLayerMethod.None, null, null, 256, 256);
      Assert.ThrowsException<ServiceException>(() => request.Validate());
    }

    [TestMethod]
    public void ValidateFailTwoBoundingBoxesTest()
    {
      var boundingBox2dGrid = new BoundingBox2DGrid(380646.982394, 812634.205106, 380712.19834, 812788.92875);

      //Two bounding boxes
      TileRequest request = new TileRequest(projectId, null, callId, DisplayMode.Height, null, liftSettings, VolumesType.None, 0.0, null, null, 0, null, 0, FilterLayerMethod.None, boundingBox2DLatLon, boundingBox2dGrid, 256, 256);
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

    private readonly List<ColorPalette> palettes = new List<ColorPalette>
    {
      new ColorPalette(Red, 0.0),
      new ColorPalette(Lime, 90.0),
      new ColorPalette(Blue, 120.0)
    };

    private readonly List<ColorPalette> invalidPalettes = new List<ColorPalette>
    {
      new ColorPalette(Lime, 90.0),
      new ColorPalette(Red, 0.0),
      new ColorPalette(Blue, 120.0)
    };

    //These are from 3dpm Colors class, copied here as only for unit tests and actually any colors will do
    const uint Red = 0xFF0000;
    const uint Lime = 0x00FF00;
    const uint Blue = 0x0000FF;
  }
}
