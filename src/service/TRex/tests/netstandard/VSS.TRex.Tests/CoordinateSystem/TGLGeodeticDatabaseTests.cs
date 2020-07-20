using System.IO;
using FluentAssertions;
using Xunit;

namespace VSS.TRex.Tests.CoordinateSystem
{
  public class TGLGeodeticDatabaseTests
  {
    [Fact]
    public void Geodetic_database_folder_should_contain_TGL_content()
    {
      var geodeticDirectory = CoreX.Wrapper.CoreX.GeodeticDatabasePath;

      if (Directory.Exists(geodeticDirectory))
      {
        // Crude check that at least some geodetic database files are present.
        Directory.GetFiles(geodeticDirectory, "*.dgf").Length.Should().BeGreaterThan(0);
        Directory.GetFiles(geodeticDirectory, "*.ggf").Length.Should().BeGreaterThan(0);
        Directory.GetFiles(geodeticDirectory, "*.gml").Length.Should().BeGreaterThan(0);
        Directory.GetFiles(geodeticDirectory, "*.mrp").Length.Should().BeGreaterThan(0);
        Directory.GetFiles(geodeticDirectory, "*.pjg").Length.Should().BeGreaterThan(0);
        Directory.GetFiles(geodeticDirectory, "*.sgf").Length.Should().BeGreaterThan(0);
      }
    }
  }
}
