using System.IO;

namespace WebApiTests
{
  public class TestFile
  {
    private const string ImportFilePath = "FileImportFiles";

    public static string TestAlignment1 => Path.Combine(ImportFilePath, "TestAlignment1.svl");
    public static string TestAlignment2 => Path.Combine(ImportFilePath, "TestAlignment2.SVL");
    public static string TestDesignSurface1 => Path.Combine(ImportFilePath, "TestDesignSurface1.ttm");
    public static string TestDesignSurface2 => Path.Combine(ImportFilePath, "TestDesignSurface2.TTM");
  }
}