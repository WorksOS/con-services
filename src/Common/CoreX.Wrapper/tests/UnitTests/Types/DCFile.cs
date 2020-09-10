using System.IO;

namespace CoreX.Wrapper.UnitTests.Types
{
  public static class DCFile
  {
    public const string DIMENSIONS_2012_DC_FILE_WITHOUT_VERT_ADJUST = "BootCamp_2012_WithoutVertAdjust.dc";
    public const string DIMENSIONS_2012_DC_FILE_WITH_VERT_ADJUST = "BootCamp_2012_WithVertAdjust.dc";
    public const string UTM_32_NN1954_08 = "UTM_32_NN1954-08.dc";
    public const string NETHERLANDS_DE_MIN = "Netherlands (De Min).dc";
    public const string NETHERLANDS_NO_GEOID = "Netherlands (No geoid).dc";
    public const string NN2000_NORWAY18A = "Haaneskrysset.dc";
    public const string PHILIPSBURG = "Philipsburg TVN_Z3_Bessel.dc";
    public const string FLORIDA_EAST_0901_NAD_1983 = "Florida_East_0901_NAD_1983.dc"; // AKA 7e752f8a-1b27-420a-92e6-5a78125450f0.cal

    public static string GetFilePath(string filename) => Path.Combine("CoordinateSystems", filename);
  }
}
