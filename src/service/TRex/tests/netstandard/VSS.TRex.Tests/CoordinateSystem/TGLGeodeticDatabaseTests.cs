using System.IO;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.CoordinateSystem
{
  public class TGLGeodeticDatabaseTests: IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Geodetic_database_folder_should_contain_TGL_content()
    {
      var geodeticDirectory = new CoreX.Wrapper.CoreX(DIContext.Obtain<ILoggerFactory>()).GeodeticDatabasePath;

      if (Directory.Exists(geodeticDirectory))
      {
        // Crude check that at least some geodetic database files are present.
        //Directory.GetFiles(geodeticDirectory, "*.dgf").Length.Should().BeGreaterThan(0);
        Directory.GetFiles(geodeticDirectory, "*.ggf").Length.Should().BeGreaterThan(0);
        //Directory.GetFiles(geodeticDirectory, "*.gml").Length.Should().BeGreaterThan(0);
        //Directory.GetFiles(geodeticDirectory, "*.mrp").Length.Should().BeGreaterThan(0);
        //Directory.GetFiles(geodeticDirectory, "*.pjg").Length.Should().BeGreaterThan(0);
        //Directory.GetFiles(geodeticDirectory, "*.sgf").Length.Should().BeGreaterThan(0);
      }
    }
  }
}
