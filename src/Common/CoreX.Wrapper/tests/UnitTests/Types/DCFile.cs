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

    public static string GetFilePath(string filename) => Path.Combine("CoordinateSystems", filename);
  }
}
