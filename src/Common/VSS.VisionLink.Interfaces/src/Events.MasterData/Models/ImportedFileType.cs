namespace VSS.Visionlink.Interfaces.Events.MasterData.Models
{
    public enum ImportedFileType
    {
        Linework = 0,
        DesignSurface = 1,
        SurveyedSurface = 2,
        Alignment = 3,
        MobileLinework = 4,
        SiteBoundary = 5,
        ReferenceSurface = 6,
        MassHaulPlan = 7,
        GeoTiff = 8,
        // These are the types used by CWS
        Calibration = 9,
        AvoidanceZone = 10,
        ControlPoints = 11,
        Geoid = 12,
        FeatureCode = 13,
        SiteConfiguration = 14,
        GcsCalibration= 15
  }
}
