using FluentAssertions;
using VSS.TRex.Caching;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Caching
{
  public class SpatialMemoryCacheHeartBeatLoggerTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Creation()
    {
      var logger = new SpatialMemoryCacheHeartBeatLogger();
      logger.Should().NotBeNull();
    }

    [Fact]
    public void Test_ToString()
    {
      var logger = new SpatialMemoryCacheHeartBeatLogger();
      logger.ToString().Should().Match("*General Result Cache: Item count = * Context count = *, Project count = *, Removed Contexts = *, Indicative size = *");
    }
  }
}
