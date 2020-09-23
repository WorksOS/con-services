using System;
using Trimble.CsdManagementWrapper;

namespace CoreX.Extensions
{
  public static class ICoordinateSystemExtensions
  {
    public static void ThrowError(string errorMessage) => throw new Exception(errorMessage);

    public static void Validate(this ICoordinateSystem record)
    {
      const int MISSING_ID = -1;
      const double MISSING_VALUE = -9.99e27;

      // checking names
      if (string.IsNullOrEmpty(record.SystemName())) { ThrowError("SystemName cannot be null"); }
      if (string.IsNullOrEmpty(record.ZoneName())) { ThrowError("ZoneName cannot be null"); }
      if (string.IsNullOrEmpty(record.RecordName())) { ThrowError("RecordName cannot be null"); }

      if (record.DatumType() != csmDatumTypes.cdtUnknown)
      {
        if (string.IsNullOrEmpty(record.DatumName())) { ThrowError("DatumName cannot be null"); }
        if (string.IsNullOrEmpty(record.EllipseName())) { ThrowError("EllipseName cannot be null"); }
        if (record.EllipseA() <= 0d) { ThrowError("EllipseA() must be > 0d"); }
        if (record.EllipseInverseFlat() <= 0d) { ThrowError("EllipseInverseFlat() must be > 0d"); }
        if (MISSING_ID == record.EllipseSystemId()) { ThrowError("Missing ellipseSystemId"); }
      }
      else
      {
        if (string.IsNullOrEmpty(record.DatumName())) { ThrowError("DatumName should be null"); }
        if (string.IsNullOrEmpty(record.EllipseName())) { ThrowError("EllipseName should be null"); }
      }

      // checking IDs
      if (MISSING_ID == record.SystemId()) { ThrowError("SystemId should be set"); }
      if (MISSING_ID == record.ZoneSystemId()) { ThrowError("ZoneSystemId should be set"); }

      switch (record.DatumType())
      {
        case csmDatumTypes.cdtWgs84:
          if (MISSING_ID == record.DatumSystemId()) { ThrowError("DatumSystemId should be set"); }
          break;
        case csmDatumTypes.cdtMolodensky:
          if (MISSING_ID == record.DatumSystemId()) { ThrowError("DatumSystemId should be set"); }
          if (MISSING_VALUE == record.DatumTranslationX()) { ThrowError("DatumTranslationX should be set"); }
          if (MISSING_VALUE == record.DatumTranslationY()) { ThrowError("DatumTranslationY should be set"); }
          if (MISSING_VALUE == record.DatumTranslationZ()) { ThrowError("DatumTranslationZ should be set"); }
          break;
        case csmDatumTypes.cdtSevenParameter:
          if (MISSING_ID == record.DatumSystemId()) { ThrowError("DatumSystemId should be set"); }
          if (MISSING_VALUE == record.DatumTranslationX()) { ThrowError("DatumTranslationX should be set"); }
          if (MISSING_VALUE == record.DatumTranslationY()) { ThrowError("DatumTranslationY should be set"); }
          if (MISSING_VALUE == record.DatumTranslationZ()) { ThrowError("DatumTranslationZ should be set"); }
          if (MISSING_VALUE == record.DatumRotationX()) { ThrowError("DatumRotationX should be set"); }
          if (MISSING_VALUE == record.DatumRotationY()) { ThrowError("DatumRotationY should be set"); }
          if (MISSING_VALUE == record.DatumRotationZ()) { ThrowError("DatumRotationZ should be set"); }
          if (MISSING_VALUE == record.DatumScale()) { ThrowError("DatumScale should be set"); }
          break;
        case csmDatumTypes.cdtGridDatum:
          {
            if (MISSING_ID == record.DatumSystemId()) { ThrowError("DatumSystemId should be set"); }
            if (string.IsNullOrEmpty(record.DatumLatitudeShiftGridFileName())) { ThrowError("DatumLatitudeShiftGridFileName should not be null"); }
            if (string.IsNullOrEmpty(record.DatumLongitudeShiftGridFileName())) { ThrowError("DatumLongitudeShiftGridFileName should not be null"); }

            if (record.DatumHasHeightShiftGridFile())
            {
              if (string.IsNullOrEmpty(record.DatumHeightShiftGridFileName())) { ThrowError("DatumHeightShiftGridFileName should not be null"); }
            }

            break;
          }
        case csmDatumTypes.cdtRtcmDatum:
          {
            if (MISSING_ID == record.DatumSystemId()) { ThrowError("DatumSystemId should be set"); }
            if (string.IsNullOrEmpty(record.DatumRtdFileName())) { ThrowError("DatumRtdFileName should be set"); }
            break;
          }
        case csmDatumTypes.cdtMultipleRegression:
          {
            if (MISSING_ID == record.DatumSystemId()) { ThrowError("DatumSystemId should be set"); }
            break;
          }
        case csmDatumTypes.cdtIRecordWgs84TypeUnusedHere:
          break;
        case csmDatumTypes.cdtUnknown:
          break;
        default:
          ThrowError("Unknown datum type");
          break;
      }

      if (record.HasGeoid())
      {
        if (MISSING_ID == record.GeoidSystemId()) { ThrowError("GeoidSystemId should be set"); }
        if (string.IsNullOrEmpty(record.GeoidName())) { ThrowError("GeoidName should not be null"); }
        if (string.IsNullOrEmpty(record.GeoidFileName())) { ThrowError("GeoidFileName should not be null"); }
      }

      var zoneType = record.ZoneType();

      if (zoneType == csmZoneTypes.cztUnknown) { return; }

      //  Test known values for zone types

      if (zoneType == csmZoneTypes.cztKrovakZone)
      {
        if (record.ZoneOriginLatitude() == record.ZoneOriginLongitude()) { ThrowError("ZoneOriginLatitude should not equal ZoneOriginLongitude"); }
      }

      if (zoneType != csmZoneTypes.cztGridZone &&
          zoneType != csmZoneTypes.cztNewZealandMapGridZone &&
          zoneType != csmZoneTypes.cztSnakeGrid &&
          zoneType != csmZoneTypes.cztUnitedKingdomNationalGridZone &&
          record.ZoneSystemId() != 553)
      {
        if (MISSING_VALUE == record.ZoneOriginLatitude()) { ThrowError("ZoneOriginLatitude should not be null"); }
        if (MISSING_VALUE == record.ZoneOriginLongitude()) { ThrowError("ZoneOriginLongitude should not be null"); }
        if (MISSING_VALUE == record.ZoneOriginNorth()) { ThrowError("ZoneOriginNorth should not be null"); }
        if (MISSING_VALUE == record.ZoneOriginEast()) { ThrowError("ZoneOriginEast should not be null"); }
      }
      else if (record.ZoneSystemId() == 553)
      {
        if (MISSING_VALUE == record.ZoneOriginLatitude()) { ThrowError("ZoneOriginLatitude should not be null"); }
        if (MISSING_VALUE == record.ZoneOriginNorth()) { ThrowError("ZoneOriginNorth should not be null"); }
        if (MISSING_VALUE == record.ZoneOriginEast()) { ThrowError("ZoneOriginEast should not be null"); }
      }
      else
      {
        if (zoneType == csmZoneTypes.cztUnitedKingdomNationalGridZone)
        {
          if (string.IsNullOrEmpty(record.ZoneNorthGridFileName())) { ThrowError("ZoneNorthGridFileName should not be null"); }
          if (string.IsNullOrEmpty(record.ZoneEastGridFileName())) { ThrowError("ZoneEastGridFileName should not be null"); }
        }
        else if (zoneType == csmZoneTypes.cztGridZone)
        {
          record.ZoneGridFileName();
        }
      }
      if (record.ZoneHasOriginScale())
      {
        if (MISSING_VALUE == record.ZoneOriginScale()) { ThrowError("ZoneOriginScale should not be null"); }
      }

      if (record.ZoneHasNorthParallel())
      {
        if (MISSING_VALUE == record.ZoneNorthParallel()) { ThrowError("ZoneNorthParallel should not be null"); }
      }

      if (record.ZoneHasSouthParallel())
      {
        if (MISSING_VALUE == record.ZoneSouthParallel()) { ThrowError("ZoneSouthParallel should not be null"); }
      }

      if (record.ZoneType() == csmZoneTypes.cztKrovakZone)
      {
        if (MISSING_VALUE == record.ZoneFerroConstant()) { ThrowError("ZoneFerroConstant should not be null"); }
      }

      record.ZoneOriginType();
      record.ZoneRectify();
      record.ZoneAzimuthType();
      record.ZoneDenmarkCoordinateSystem();
      if (record.ZoneHasShiftGridFile())
      {
        if (string.IsNullOrEmpty(record.ZoneShiftGridFileName())) { ThrowError("ZoneShiftGridFileName should not be null"); }
      }

      record.ZoneIsSouthGrid();
      record.ZoneIsWestGrid();
      record.ZoneIsSouthAzimuth();
      record.SnakeGridFileName();

      if (record.HasHorizAdjustment())
      {
        if (MISSING_VALUE == record.HorizAdjustmentOriginEast()) { ThrowError("HorizAdjustmentOriginEast should not be null"); }
        if (MISSING_VALUE == record.HorizAdjustmentOriginNorth()) { ThrowError("HorizAdjustmentOriginNorth should not be null"); }
        if (MISSING_VALUE == record.HorizAdjustmentRotation()) { ThrowError("HorizAdjustmentRotation should not be null"); }
        if (MISSING_VALUE == record.HorizAdjustmentScale()) { ThrowError("HorizAdjustmentScale should not be null"); }
        if (MISSING_VALUE == record.HorizAdjustmentTranslationEast()) { ThrowError("HorizAdjustmentTranslationEast should not be null"); }
        if (MISSING_VALUE == record.HorizAdjustmentTranslationNorth()) { ThrowError("HorizAdjustmentTranslationNorth should not be null"); }
      }

      if (record.HasVertAdjustment())
      {
        if (MISSING_VALUE == record.VertAdjustmentConstantAdjustment()) { ThrowError("VertAdjustmentConstantAdjustment should not be null"); }
        if (MISSING_VALUE == record.VertAdjustmentOriginEast()) { ThrowError("VertAdjustmentOriginEast should not be null"); }
        if (MISSING_VALUE == record.VertAdjustmentOriginNorth()) { ThrowError("VertAdjustmentOriginNorth should not be null"); }
        if (MISSING_VALUE == record.VertAdjustmentSlopeEast()) { ThrowError("VertAdjustmentSlopeEast should not be null"); }
        if (MISSING_VALUE == record.VertAdjustmentSlopeNorth()) { ThrowError("VertAdjustmentSlopeNorth should not be null"); }
      }

      if (record.HasValidRegion())
      {
        if (MISSING_VALUE == record.ValidRegionMaxLat()) { ThrowError("ValidRegionMaxLat should not be null"); }
        if (MISSING_VALUE == record.ValidRegionMaxLng()) { ThrowError("ValidRegionMaxLng should not be null"); }
        if (MISSING_VALUE == record.ValidRegionMinLat()) { ThrowError("ValidRegionMinLat should not be null"); }
        if (MISSING_VALUE == record.ValidRegionMinLng()) { ThrowError("ValidRegionMinLng should not be null"); }
      }

      if (record.HasLocalSiteParameters())
      {
        if (MISSING_VALUE == record.LocalSiteNorthingOffset()) { ThrowError("LocalSiteNorthingOffset should not be null"); }
        if (MISSING_VALUE == record.LocalSiteEastingOffset()) { ThrowError("LocalSiteEastingOffset should not be null"); }
        if (MISSING_VALUE == record.LocalSiteGroundScaleFactor()) { ThrowError("LocalSiteGroundScaleFactor should not be null"); }

        if (record.LocalSiteHasLocation())
        {
          if (MISSING_VALUE == record.LocalSiteLocationLatitude()) { ThrowError("LocalSiteLocationLatitude should not be null"); }
          if (MISSING_VALUE == record.LocalSiteLocationLongitude()) { ThrowError("LocalSiteLocationLongitude should not be null"); }
          if (MISSING_VALUE == record.LocalSiteLocationHeight()) { ThrowError("LocalSiteLocationHeight should not be null"); }

          if (record.LocalSiteIsGroundScaleFactorComputed())
          {
          }
        }
        else
        {
          if (record.LocalSiteIsGroundScaleFactorComputed()) { ThrowError("LocalSiteIsGroundScaleFactorComputed should be false"); }
        }
      }
    }
  }
}
