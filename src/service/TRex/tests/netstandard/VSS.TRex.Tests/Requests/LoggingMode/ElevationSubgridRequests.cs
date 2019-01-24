using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.TAGFiles.Classes.Integrator;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Requests.LoggingMode
{
  /// <summary>
  /// This test class exercises reading TAG files containing lowest elevation mapping mode states into a ephemeral site model and
  /// then querying elevation information from those cell passes to verify expected selection of cell passes based on
  /// the recorded elevation mapping mode
  /// </summary>
  public class ElevationSubgridRequests : IClassFixture<DITAGFileAndSubGridRequestsFixture>
  {
    private ISiteModel ConstructModelForTestsWithTwoExcavatorMachineTAGFiles(out List<AggregatedDataIntegratorTask> processedTasks)
    {
      var tagFiles = new[] {Path.Combine("TestData", "TAGFiles", "ElevationMappingMode-KettlewellDrive", "0187J008YU--TNZ 323F GS520--190123002153.tag"),
                            Path.Combine("TestData", "TAGFiles", "ElevationMappingMode-KettlewellDrive", "0187J008YU--TNZ 323F GS520--190123002153.tag")};
      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out processedTasks);

      siteModel.Should().NotBeNull();
      processedTasks.Count.Should().Be(tagFiles.Length);

      siteModel.Grid.CountLeafSubgridsInMemory().Should().Be(4);
      siteModel.MachinesTargetValues[0].ElevationMappingModeStateEvents.Count().Should().Be(2);
      siteModel.Grid.Root.ForEachSubGrid(subGrid =>
      {
        subGrid.ForEach((x, y) => { });
        return SubGridProcessNodeSubGridResult.OK;
      });

      return siteModel;
    }

    [Fact]
    public void Test_ElevationSubgridRequests_ModelConstruction()
    {
      var siteModel = ConstructModelForTestsWithTwoExcavatorMachineTAGFiles(out var processedTasks);
      siteModel.Should().NotBeNull();
    }

    [Fact]
    public void Test_ElevationSubgridRequests_RequestElevationSubGrids()
    {
      var siteModel = ConstructModelForTestsWithTwoExcavatorMachineTAGFiles(out var processedTasks);


    }
  }
}
