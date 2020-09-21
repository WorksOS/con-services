using System.IO;
using VSS.TRex.Analytics.CMVChangeStatistics.GridFabric;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace TAGFiles.Tests
{
  public class EventListTests : BaseTests<CMVChangeStatisticsArgument, CMVChangeStatisticsResponse>, IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {

    [Fact]
    public void Test_CollateOfVibrationStateEvents()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var tagFiles = new[]
      {
        Path.Combine("TestData", "TAGFiles", "TestTAGFile-CMV-1.tag"),
        Path.Combine("TestData", "TAGFiles", "TestTAGFile-CMV-2.tag")
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);
      Assert.True(siteModel.MachinesTargetValues[0].VibrationStateEvents.Count() == 9, "Should be 9 vibration state events");
    }
  }
}
