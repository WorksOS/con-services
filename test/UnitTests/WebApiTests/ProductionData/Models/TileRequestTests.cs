using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.ProductionData.Models
{
  [TestClass]
  public class TileRequestTests
  {
    [TestMethod]
    public void CanCreateTileRequestTest()
    {

      var validator = new DataAnnotationsValidator();
      TileRequest request = TileRequest.CreateTileRequest(
                projectId, callId, DisplayMode.Height, null, liftSettings, RaptorConverters.VolumesType.None, 0.0, null, null, 0, null, 0,
                FilterLayerMethod.None, BoundingBox2DLatLon.HelpSample, null, 256, 256);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(request, out results));

      //missing project id
      request = TileRequest.CreateTileRequest(-1, callId, DisplayMode.Height, null, liftSettings, RaptorConverters.VolumesType.None, 0.0, null, null, 0, null, 0,
                FilterLayerMethod.None, BoundingBox2DLatLon.HelpSample, null, 256, 256);
      Assert.IsFalse(validator.TryValidate(request, out results));

      //vol no change tolerance out of range
      request = TileRequest.CreateTileRequest(projectId, callId, DisplayMode.Height, null, liftSettings, RaptorConverters.VolumesType.None, 10.1, null, null, 0, null, 0,
                FilterLayerMethod.None, BoundingBox2DLatLon.HelpSample, null, 256, 256);
      Assert.IsFalse(validator.TryValidate(request, out results));

      //width out of range
      request = TileRequest.CreateTileRequest(projectId, callId, DisplayMode.Height, null, liftSettings, RaptorConverters.VolumesType.None, 0.0, null, null, 0, null, 0,
                FilterLayerMethod.None, BoundingBox2DLatLon.HelpSample, null, 16, 256);
      Assert.IsFalse(validator.TryValidate(request, out results));

      //height out of range
      request = TileRequest.CreateTileRequest(projectId, callId, DisplayMode.Height, null, liftSettings, RaptorConverters.VolumesType.None, 0.0, null, null, 0, null, 0,
                FilterLayerMethod.None, BoundingBox2DLatLon.HelpSample, null, 256, 16000);
      Assert.IsFalse(validator.TryValidate(request, out results));

    }

    [TestMethod]
    public void ValidateSuccessTest()
    {
      TileRequest request = TileRequest.CreateTileRequest(projectId, callId, DisplayMode.Height, null, liftSettings, RaptorConverters.VolumesType.None, 0.0, null, null, 0, null, 0,
                FilterLayerMethod.None, BoundingBox2DLatLon.HelpSample, null, 256, 256);
      request.Validate();

    }


    [TestMethod]
    public void ValidateFailInvalidPaletteNumberTest()
    {
      //wrong number of palettes for display mode
      TileRequest request = TileRequest.CreateTileRequest(projectId, callId, DisplayMode.CCVPercent, palettes, liftSettings, RaptorConverters.VolumesType.None, 0.0, null, null, 0, null, 0,
                FilterLayerMethod.None, BoundingBox2DLatLon.HelpSample, null, 256, 256);
      Assert.ThrowsException<ServiceException>(() => request.Validate());
    }

    [TestMethod]
    public void ValidateFailInvalidPaletteOrderTest()
    {
      //palettes out of order
      TileRequest request = TileRequest.CreateTileRequest(projectId, callId, DisplayMode.Height, invalidPalettes, liftSettings, RaptorConverters.VolumesType.None, 0.0, null, null, 0, null, 0,
                FilterLayerMethod.None, BoundingBox2DLatLon.HelpSample, null, 256, 256);
      Assert.ThrowsException<ServiceException>(() => request.Validate());
    }


    [TestMethod]
    public void ValidateFailMissingDesignTest()
    {
      //missing design (for cutfill display mode)
      TileRequest request = TileRequest.CreateTileRequest(projectId, callId, DisplayMode.CutFill, null, liftSettings,
        RaptorConverters.VolumesType.BetweenDesignAndFilter, 0.0, null, null, 0, null, 0,
        FilterLayerMethod.None, BoundingBox2DLatLon.HelpSample, null, 256, 256);
      Assert.ThrowsException<ServiceException>(() => request.Validate());
    }


    [TestMethod]
    public void ValidateFailMissingFilterTest()
    {
      //missing filter (for volumes display mode)
      TileRequest request = TileRequest.CreateTileRequest(projectId, callId, DisplayMode.VolumeCoverage, null, liftSettings, RaptorConverters.VolumesType.Between2Filters, 0.0, null, null, 0, null, 0,
                FilterLayerMethod.None, BoundingBox2DLatLon.HelpSample, null, 256, 256);
      Assert.ThrowsException<ServiceException>(() => request.Validate());
    }

    [TestMethod]
    public void ValidateFailInvalidVolumeTypeTest()
    {
      //Unsupported volume type (for volumes display mode)
      TileRequest request = TileRequest.CreateTileRequest(projectId, callId, DisplayMode.VolumeCoverage, null, liftSettings, RaptorConverters.VolumesType.AboveLevel, 0.0, null, null, 0, null, 0,
                FilterLayerMethod.None, BoundingBox2DLatLon.HelpSample, null, 256, 256);
      Assert.ThrowsException<ServiceException>(() => request.Validate());
    }

    [TestMethod]
    public void ValidateFailMissingBoundingBoxTest()
    {
      //missing bounding box
      TileRequest request = TileRequest.CreateTileRequest(projectId, callId, DisplayMode.Height, null, liftSettings, RaptorConverters.VolumesType.None, 0.0, null, null, 0, null, 0,
                FilterLayerMethod.None, null, null, 256, 256);
      Assert.ThrowsException<ServiceException>(() => request.Validate());
    }

    [TestMethod]
    public void ValidateFailTwoBoundingBoxesTest()
    {
      //Two bounding boxes
      TileRequest request = TileRequest.CreateTileRequest(projectId, callId, DisplayMode.Height, null, liftSettings, RaptorConverters.VolumesType.None, 0.0, null, null, 0, null, 0,
                FilterLayerMethod.None, BoundingBox2DLatLon.HelpSample, BoundingBox2DGrid.HelpSample, 256, 256);
      Assert.ThrowsException<ServiceException>(() => request.Validate());
    }

    private long projectId = 1234;
    private Guid callId = new Guid();
    private LiftBuildSettings liftSettings = LiftBuildSettings.CreateLiftBuildSettings(
      CCVRangePercentage.CreateCcvRangePercentage(80, 110), false, 1.0, 2.0, 0.2f, LiftDetectionType.Automatic, LiftThicknessType.Compacted,
      MDPRangePercentage.CreateMdpRangePercentage(70, 120), false, null, null, null, null, null, null, LiftThicknessTarget.HelpSample, null);

    private List<ColorPalette> palettes = new List<ColorPalette>
                                         {
                                           ColorPalette.CreateColorPalette(Colors.Red, 0.0),
                                           ColorPalette.CreateColorPalette(Colors.Lime, 90.0),
                                           ColorPalette.CreateColorPalette(Colors.Blue, 120.0)
                                         };

    private List<ColorPalette> invalidPalettes = new List<ColorPalette>
                                         {
                                           ColorPalette.CreateColorPalette(Colors.Lime, 90.0),
                                           ColorPalette.CreateColorPalette(Colors.Red, 0.0),
                                           ColorPalette.CreateColorPalette(Colors.Blue, 120.0)
                                         };
    private DesignDescriptor design = DesignDescriptor.CreateDesignDescriptor(1, null, 0);




  }
}
