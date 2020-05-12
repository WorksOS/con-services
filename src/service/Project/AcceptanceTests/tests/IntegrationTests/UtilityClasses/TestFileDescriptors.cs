namespace IntegrationTests.UtilityClasses
{
  public static class TestFile
  {
    // Files referenced here should be resolved using the TestUtility class TestFileResolver. This gets
    // around the FlowJS file upload handler bug where doesn't support concurrent requests that use the same
    // filename.

    public static string TestAlignment1 = "TestAlignment1.svl";
    public static string TestAlignment2 = "TestAlignment2.SVL";
    public static string TestDesignSurface1 = "TestDesignSurface1.ttm";
    public static string TestDesignSurface2 = "TestDesignSurface2.TTM";
    public static string TestDesignSurface3_GoodContent = "TestDesignSurface3_GoodContent.TTM";
    public static string TestDxFfile = "MillingDesignMap.dxf";
    public static string TestGeotiffFile = "cea.tif";
    public static string TestAvoidanceZone1 = "site.avoid.svl";
    public static string TestAvoidanceZone2 = "overpass.avoid.dxf";
    public static string TestControlPoints1 = "control.V01.cpz";
    public static string TestControlPoints2 = "overpass.office.csv";
    public static string TestGeoid = "NZGD05.ggf";
    public static string TestFeatureCode = "GlobalFeatures.fxl";
    public static string TestSiteConfiguration = "dimensions.xml";
    public static string TestGcsCalibration = "Mount Pleasant 2000.cfg";
    public static string TestCalibration1 = "BelfastTemp Road.dc";
    public static string TestCalibration2 = "Belfast temporary road.cal";
  }
}
