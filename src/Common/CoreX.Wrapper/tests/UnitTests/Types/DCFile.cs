using System.IO;
using System.Text;

namespace CoreX.Wrapper.UnitTests.Types
{
  public static class DCFile
  {
    public const string DIMENSIONS_2012_DC_FILE_WITHOUT_VERT_ADJUST = "BootCamp_2012_WithoutVertAdjust.dc";
    public const string DIMENSIONS_2012_DC_FILE_WITH_VERT_ADJUST = "BootCamp_2012_WithVertAdjust.dc";
    public const string UTM_32_NN1954_08 = "UTM_32_NN1954-08.dc";

    public static string GetFilePath(string filename) => Path.Combine("TestData", "CoordinateSystems", filename);

    public static string GetDCFileContent(string filename)
    {
      using var streamReader = new StreamReader(new FileStream(GetFilePath(filename), FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF8);
      return streamReader.ReadToEnd();
    }
  }
}
