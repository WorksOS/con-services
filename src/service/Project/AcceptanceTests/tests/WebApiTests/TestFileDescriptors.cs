using System.IO;

namespace WebApiTests
{
  public class TestFile
  {
    public static string TestAlignment1 = "TestAlignment1.svl";
    public static string TestAlignment2 = "TestAlignment2.SVL";
    public static string TestDesignSurface1 = "TestDesignSurface1.ttm";
    public static string TestDesignSurface2 = "TestDesignSurface2.TTM";
    public static string TestDxFfile = "MillingDesignMap.dxf";
  }

  public static class StringExtensions
  {
    public static string FullPath(this string testFile)
    {
      return Path.Combine("FileImportFiles", testFile);
    }
  }
}
