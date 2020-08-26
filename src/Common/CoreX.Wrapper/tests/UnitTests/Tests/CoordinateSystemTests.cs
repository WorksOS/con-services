using CoreX.Interfaces;
using CoreX.Wrapper.Types;
using CoreX.Wrapper.UnitTests.Types;
using FluentAssertions;
using Trimble.CsdManagementWrapper;
using Xunit;

namespace CoreX.Wrapper.UnitTests.Tests
{
  public class CoordinateSystemTests : IClassFixture<UnitTestBaseFixture>
  {
    private readonly ICoreXWrapper _coreX;

    public CoordinateSystemTests(UnitTestBaseFixture testFixture)
    {
      _coreX = testFixture.CoreXWrapper;
    }

    [Theory]
    //[InlineData("85,World wide/UTM", "838,10 North")]
    [InlineData("2597,Tonga/GD2005", "2598,Tonga Map Grid", CSIB.TONGA_2598_MAP_GRID)]
    public void Should_throw_when_geodetic_datafile_not_found(string zoneGroupName, string zoneName, string expectedCsib)
    {
      var csib = _coreX.GetCoordinateSystemFromCSDSelection(zoneGroupName, zoneName);

      csib.Should().Be(expectedCsib);
    }

    private void VerifyRecord(ICoordinateSystem record)
    {
      // checking names
      Assert.False(string.IsNullOrEmpty(record.SystemName()));
      Assert.False(string.IsNullOrEmpty(record.ZoneName()));
      Assert.False(string.IsNullOrEmpty(record.RecordName()));

      if (record.DatumType() != csmDatumTypes.cdtUnknown)
      {
        Assert.False(string.IsNullOrEmpty(record.DatumName()));
        Assert.False(string.IsNullOrEmpty(record.EllipseName()));
        Assert.True(record.EllipseA() > 0d);
        Assert.True(record.EllipseInverseFlat() > 0d);
        Assert.NotEqual(Utils.MISSING_ID, record.EllipseSystemId());
      }
      else
      {
        Assert.True(string.IsNullOrEmpty(record.DatumName()));
        Assert.True(string.IsNullOrEmpty(record.EllipseName()));
      }

      // checking IDs
      Assert.NotEqual(Utils.MISSING_ID, record.SystemId());
      Assert.NotEqual(Utils.MISSING_ID, record.ZoneSystemId());

      switch (record.DatumType())
      {
        case csmDatumTypes.cdtWgs84:
          Assert.NotEqual(Utils.MISSING_ID, record.DatumSystemId());
          break;
        case csmDatumTypes.cdtMolodensky:
          Assert.NotEqual(Utils.MISSING_ID, record.DatumSystemId());
          Assert.NotEqual(Utils.MISSING_VALUE, record.DatumTranslationX());
          Assert.NotEqual(Utils.MISSING_VALUE, record.DatumTranslationY());
          Assert.NotEqual(Utils.MISSING_VALUE, record.DatumTranslationZ());
          break;
        case csmDatumTypes.cdtSevenParameter:
          Assert.NotEqual(Utils.MISSING_ID, record.DatumSystemId());
          Assert.NotEqual(Utils.MISSING_VALUE, record.DatumTranslationX());
          Assert.NotEqual(Utils.MISSING_VALUE, record.DatumTranslationY());
          Assert.NotEqual(Utils.MISSING_VALUE, record.DatumTranslationZ());
          Assert.NotEqual(Utils.MISSING_VALUE, record.DatumRotationX());
          Assert.NotEqual(Utils.MISSING_VALUE, record.DatumRotationY());
          Assert.NotEqual(Utils.MISSING_VALUE, record.DatumRotationZ());
          Assert.NotEqual(Utils.MISSING_VALUE, record.DatumScale());
          break;
        case csmDatumTypes.cdtGridDatum:
          {
            Assert.NotEqual(Utils.MISSING_ID, record.DatumSystemId());
            Assert.False(string.IsNullOrEmpty(record.DatumLatitudeShiftGridFileName()));
            Assert.False(string.IsNullOrEmpty(record.DatumLongitudeShiftGridFileName()));

            if (record.DatumHasHeightShiftGridFile())
            {
              Assert.False(string.IsNullOrEmpty(record.DatumHeightShiftGridFileName()));
            }

            break;
          }
        case csmDatumTypes.cdtRtcmDatum:
          {
            Assert.NotEqual(Utils.MISSING_ID, record.DatumSystemId());
            Assert.False(string.IsNullOrEmpty(record.DatumRtdFileName()));
            break;
          }
        case csmDatumTypes.cdtMultipleRegression:
          {
            Assert.NotEqual(Utils.MISSING_ID, record.DatumSystemId());
            break;
          }
        case csmDatumTypes.cdtIRecordWgs84TypeUnusedHere:
          break;
        case csmDatumTypes.cdtUnknown:
          break;
        default:
          Assert.True(false);
          break;
      }

      if (record.HasGeoid())
      {
        Assert.NotEqual(Utils.MISSING_ID, record.GeoidSystemId());
        Assert.False(string.IsNullOrEmpty(record.GeoidName()));
        Assert.False(string.IsNullOrEmpty(record.GeoidFileName()));
      }

      var zoneType = record.ZoneType();

      if (zoneType == csmZoneTypes.cztUnknown)
        return;

      //  Test known values for zone types

      if (zoneType == csmZoneTypes.cztKrovakZone)
      {
        Assert.NotEqual(record.ZoneOriginLatitude(), record.ZoneOriginLongitude());
      }

      if (zoneType != csmZoneTypes.cztGridZone
          && zoneType != csmZoneTypes.cztNewZealandMapGridZone
          && zoneType != csmZoneTypes.cztSnakeGrid
          && zoneType != csmZoneTypes.cztUnitedKingdomNationalGridZone
          && record.ZoneSystemId() != 553)
      {
        Assert.NotEqual(Utils.MISSING_VALUE, record.ZoneOriginLatitude());
        Assert.NotEqual(Utils.MISSING_VALUE, record.ZoneOriginLongitude());
        Assert.NotEqual(Utils.MISSING_VALUE, record.ZoneOriginNorth());
        Assert.NotEqual(Utils.MISSING_VALUE, record.ZoneOriginEast());
      }
      else if (record.ZoneSystemId() == 553)
      {
        Assert.NotEqual(Utils.MISSING_VALUE, record.ZoneOriginLatitude());
        Assert.NotEqual(Utils.MISSING_VALUE, record.ZoneOriginNorth());
        Assert.NotEqual(Utils.MISSING_VALUE, record.ZoneOriginEast());
      }
      else
      {
        if (zoneType == csmZoneTypes.cztUnitedKingdomNationalGridZone)
        {
          Assert.False(string.IsNullOrEmpty(record.ZoneNorthGridFileName()));
          Assert.False(string.IsNullOrEmpty(record.ZoneEastGridFileName()));
        }
        else if (zoneType == csmZoneTypes.cztGridZone)
        {
          record.ZoneGridFileName();
        }
      }
      if (record.ZoneHasOriginScale())
        Assert.NotEqual(Utils.MISSING_VALUE, record.ZoneOriginScale());
      if (record.ZoneHasNorthParallel())
        Assert.NotEqual(Utils.MISSING_VALUE, record.ZoneNorthParallel());
      if (record.ZoneHasSouthParallel())
        Assert.NotEqual(Utils.MISSING_VALUE, record.ZoneSouthParallel());
      if (record.ZoneType() == csmZoneTypes.cztKrovakZone)
        Assert.NotEqual(Utils.MISSING_VALUE, record.ZoneFerroConstant());
      record.ZoneOriginType();
      record.ZoneRectify();
      record.ZoneAzimuthType();
      record.ZoneDenmarkCoordinateSystem();
      if (record.ZoneHasShiftGridFile())
        Assert.False(string.IsNullOrEmpty(record.ZoneShiftGridFileName()));
      record.ZoneIsSouthGrid();
      record.ZoneIsWestGrid();
      record.ZoneIsSouthAzimuth();
      record.SnakeGridFileName();

      if (record.HasHorizAdjustment())
      {
        Assert.NotEqual(Utils.MISSING_VALUE, record.HorizAdjustmentOriginEast());
        Assert.NotEqual(Utils.MISSING_VALUE, record.HorizAdjustmentOriginNorth());
        Assert.NotEqual(Utils.MISSING_VALUE, record.HorizAdjustmentRotation());
        Assert.NotEqual(Utils.MISSING_VALUE, record.HorizAdjustmentScale());
        Assert.NotEqual(Utils.MISSING_VALUE, record.HorizAdjustmentTranslationEast());
        Assert.NotEqual(Utils.MISSING_VALUE, record.HorizAdjustmentTranslationNorth());
      }

      if (record.HasVertAdjustment())
      {
        Assert.NotEqual(Utils.MISSING_VALUE, record.VertAdjustmentConstantAdjustment());
        Assert.NotEqual(Utils.MISSING_VALUE, record.VertAdjustmentOriginEast());
        Assert.NotEqual(Utils.MISSING_VALUE, record.VertAdjustmentOriginNorth());
        Assert.NotEqual(Utils.MISSING_VALUE, record.VertAdjustmentSlopeEast());
        Assert.NotEqual(Utils.MISSING_VALUE, record.VertAdjustmentSlopeNorth());
      }

      if (record.HasValidRegion())
      {
        Assert.NotEqual(Utils.MISSING_VALUE, record.ValidRegionMaxLat());
        Assert.NotEqual(Utils.MISSING_VALUE, record.ValidRegionMaxLng());
        Assert.NotEqual(Utils.MISSING_VALUE, record.ValidRegionMinLat());
        Assert.NotEqual(Utils.MISSING_VALUE, record.ValidRegionMinLng());
      }

      if (record.HasLocalSiteParameters())
      {
        Assert.NotEqual(Utils.MISSING_VALUE, record.LocalSiteNorthingOffset());
        Assert.NotEqual(Utils.MISSING_VALUE, record.LocalSiteEastingOffset());
        Assert.NotEqual(Utils.MISSING_VALUE, record.LocalSiteGroundScaleFactor());

        if (record.LocalSiteHasLocation())
        {
          Assert.NotEqual(Utils.MISSING_VALUE, record.LocalSiteLocationLatitude());
          Assert.NotEqual(Utils.MISSING_VALUE, record.LocalSiteLocationLongitude());
          Assert.NotEqual(Utils.MISSING_VALUE, record.LocalSiteLocationHeight());

          if (record.LocalSiteIsGroundScaleFactorComputed())
          {
          }
        }
        else
        {
          Assert.False(record.LocalSiteIsGroundScaleFactorComputed());
        }
      }
    }

  }
}
