using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using VSS.TRex.Common;
using VSS.TRex.Common.Types;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.TAGFiles.Classes.Integrator;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
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
      const int expectedSubgrids = 4;
      const int expectedEvents = 2;
      const int expectedNonNullCells = 427;

      var corePath = Path.Combine("TestData", "TAGFiles", "ElevationMappingMode-KettlewellDrive");
      var tagFiles = new[]
      {
        Path.Combine(corePath, "0187J008YU--TNZ 323F GS520--190123002153.tag"),
        Path.Combine(corePath, "0187J008YU--TNZ 323F GS520--190123002153.tag")
      };
      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out processedTasks);

      siteModel.Should().NotBeNull();
      processedTasks.Count.Should().Be(tagFiles.Length);

      siteModel.Grid.CountLeafSubgridsInMemory().Should().Be(expectedSubgrids);

      siteModel.MachinesTargetValues[0].ElevationMappingModeStateEvents.Count().Should().Be(expectedEvents);
      siteModel.MachinesTargetValues[0].ElevationMappingModeStateEvents.LastStateValue().Should().Be(ElevationMappingMode.MinimumElevation);

      // Count the number of non-null elevation cells
      long totalCells = 0;
      siteModel.Grid.Root.ScanSubGrids(siteModel.Grid.FullCellExtent(),
        subGrid =>
        {
          totalCells += ((IServerLeafSubGrid) subGrid).Directory.GlobalLatestCells.PassDataExistenceMap.CountBits();
          return true;
        });
      totalCells.Should().Be(expectedNonNullCells);

      return siteModel;
    }

    [Fact]
    public void Test_ElevationSubgridRequests_ModelConstruction()
    {
      var siteModel = ConstructModelForTestsWithTwoExcavatorMachineTAGFiles(out var processedTasks);
      siteModel.Should().NotBeNull();
    }

    [Fact]
    public void Test_ElevationSubgridRequests_RequestElevationSubGrids_NoSurveyedSurfaces_NoFilter()
    {
      var siteModel = ConstructModelForTestsWithTwoExcavatorMachineTAGFiles(out var processedTasks);

      // Construct the set of requestors to query elevation sub grids needed for the summary volume calculations.
      var utilities = DIContext.Obtain<IRequestorUtilities>();

      var Requestors = utilities.ConstructRequestors(siteModel,
        utilities.ConstructRequestorIntermediaries(siteModel, new FilterSet(new CombinedFilter()), true, GridDataType.Height),
        AreaControlSet.CreateAreaControlSet(), siteModel.ExistenceMap);

      Requestors.Should().NotBeNull();
      Requestors.Length.Should().Be(1);

      // Request all elevation sub grids from the model
      var requestedSubGrids = new List<IClientLeafSubGrid>();
      siteModel.ExistenceMap.ScanAllSetBitsAsSubGridAddresses(x =>
      {
        IClientLeafSubGrid clientGrid = DIContext.Obtain<IClientLeafSubGridFactory>().GetSubGrid(GridDataType.Height);
        if (Requestors[0].RequestSubGridInternal(x, true, false, clientGrid) == ServerRequestResult.NoError)
          requestedSubGrids.Add(clientGrid);
      });

      requestedSubGrids.Count.Should().Be(4);

      (requestedSubGrids[0] as IClientHeightLeafSubGrid).Cells[0, 0].Should().Be(Consts.NullHeight);

      long nonNullCount = 0;
      foreach (var subGrid in requestedSubGrids)
      {
        var leaf = subGrid as IClientHeightLeafSubGrid;
        SubGridUtilities.SubGridDimensionalIterator((x, y) => nonNullCount += leaf.Cells[x, y] == Consts.NullHeight ? 0 : 1);
      }

      nonNullCount.Should().Be(427);
    }
  }
}
