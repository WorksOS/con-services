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
  }
}
