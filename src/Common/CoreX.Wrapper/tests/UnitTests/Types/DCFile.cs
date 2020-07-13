using System.IO;
using System.Text;

namespace CoreX.Wrapper.UnitTests.Types
{
  public static class DCFile
  {
    public const string BOOTCAMP_2012 = "BootCamp 2012.dc";
    public const string UTM_32_NN1954_08 = "UTM_32_NN1954-08.dc";

    public static string GetFilePath(string filename) => Path.Combine("TestData", "CoordinateSystems", filename);

    public static string GetDCFileContent(string filename)
    {
      using var streamReader = new StreamReader(new FileStream(GetFilePath(filename), FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF8);
      return streamReader.ReadToEnd();
    }
  }
}
