using System.IO;

namespace VSS.TRex.Tests
{
  public static class TestHelper
  {
    /// <summary>
    /// The name of the path containing test data relative to the root path of the running
    /// test solution
    /// </summary>
    public const string TEST_DATA_LOCATION = "TestData";

    /// <summary>
    /// The name of the path containing test data relative to the root path of the running
    /// test solution
    /// </summary>
    public const string COMMON_TEST_DATA_FOLDER_NAME = "Common";

    /// <summary>
    /// The typical tolerance to use when using Should.BeApproximately() for float values describing heights/elevations
    /// </summary>
    public const float ALLOWED_HEIGHT_TOLERANCE_AS_FLOAT = 0.001f;

    /// <summary>
    /// The typical tolerance to use when using Should.BeApproximately() for double values describing heights/elevations
    /// </summary>
    public const double ALLOWED_HEIGHT_TOLERANCE_AS_DOUBLE = 0.001;

    public static string CommonTestDataPath => Path.Combine(TEST_DATA_LOCATION, COMMON_TEST_DATA_FOLDER_NAME);
  }
}
