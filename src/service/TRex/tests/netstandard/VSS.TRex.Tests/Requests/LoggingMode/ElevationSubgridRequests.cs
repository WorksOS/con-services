using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.Types;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Types;
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
  public class ElevationSubGridRequests : IClassFixture<DITAGFileAndSubGridRequestsFixture>
  {
    private readonly DateTime BASE_TIME = DateTime.Now;

    private const int TIME_INCREMENT_SECONDS = 10; // seconds
    private const float BASE_HEIGHT = 100.0f;
    private const float HEIGHT_DECREMENT = -0.1f;
    private const float MAXIMUM_HEIGHT = BASE_HEIGHT;
    private const float MINIMUM_HEIGHT = BASE_HEIGHT + HEIGHT_DECREMENT * (PASSES_IN_DECREMENTING_ELEVATION_LIST - 1);

    private const int PASSES_IN_DECREMENTING_ELEVATION_LIST = 3;

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

      siteModel.Grid.CountLeafSubgridsInMemory().Should().Be(expectedSubgrids);

      // Ensure there are two appropriate elevation mapping mode events
      siteModel.MachinesTargetValues[0].ElevationMappingModeStateEvents.Count().Should().Be(expectedEvents);
      siteModel.MachinesTargetValues[0].ElevationMappingModeStateEvents.GetStateAtIndex(0, out var eventDate, out var eventState);
      eventState.Should().Be(ElevationMappingMode.LatestElevation);
      siteModel.MachinesTargetValues[0].ElevationMappingModeStateEvents.LastStateValue().Should().Be(ElevationMappingMode.MinimumElevation);

      // Count the number of non-null elevation cells per the sub grid pass dta existence maps
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
    public void Test_ElevationSubGridRequests_ModelConstruction()
    {
      var siteModel = ConstructModelForTestsWithTwoExcavatorMachineTAGFiles(out var processedTasks);
      siteModel.Should().NotBeNull();
    }

    [Fact]
    public void Test_ElevationSubGridRequests_RequestElevationSubGrids_NoSurveyedSurfaces_NoFilter()
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

      requestedSubGrids.Cast<IClientHeightLeafSubGrid>().Sum(x => x.CountNonNullCells()).Should().Be(427);
    }

    private ISiteModel CreateSiteModelWithSingleCellForTesting()
    {
      // Set up a model with a single sub grid with a single cell containing two cell passes and a single
      // elevation mapping event with a state of lowest elevation mapping
      // Create the site model and machine etc to aggregate the processed TAG file into

      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine = siteModel.Machines.CreateNew("Test Machine", "", 1, 1, false, Guid.NewGuid());

      // Add the lowest pass elevation mapping event occurring after a last pass mapping event
      siteModel.MachinesTargetValues[0].ElevationMappingModeStateEvents.PutValueAtDate(BASE_TIME.AddSeconds(-1), ElevationMappingMode.LatestElevation);
      siteModel.MachinesTargetValues[0].ElevationMappingModeStateEvents.PutValueAtDate(BASE_TIME, ElevationMappingMode.MinimumElevation);

      // Ensure there are two appropriate elevation mapping mode events
      siteModel.MachinesTargetValues[0].ElevationMappingModeStateEvents.Count().Should().Be(2);
      siteModel.MachinesTargetValues[0].ElevationMappingModeStateEvents.GetStateAtIndex(0, out var eventDate, out var eventState);
      eventState.Should().Be(ElevationMappingMode.LatestElevation);
      siteModel.MachinesTargetValues[0].ElevationMappingModeStateEvents.LastStateValue().Should().Be(ElevationMappingMode.MinimumElevation);

      // Construct the sub grid to hold the cell being tested
      IServerLeafSubGrid leaf = siteModel.Grid.ConstructPathToCell(SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridPathConstructionType.CreateLeaf) as IServerLeafSubGrid;
      leaf.Should().NotBeNull();

      leaf.AllocateLeafFullPassStacks();
      leaf.CreateDefaultSegment();
      leaf.AllocateFullPassStacks(leaf.Directory.SegmentDirectory.First());
      leaf.AllocateLeafLatestPassGrid();

      // Add the leaf to the site model existence map
      siteModel.ExistenceMap[leaf.OriginX >> SubGridTreeConsts.SubGridIndexBitsPerLevel, leaf.OriginY >> SubGridTreeConsts.SubGridIndexBitsPerLevel] = true;

      siteModel.Grid.CountLeafSubgridsInMemory().Should().Be(1);

      // Add three passes, each separated by 10 seconds and descending by 100mm each pass
      for (int i = 0; i < PASSES_IN_DECREMENTING_ELEVATION_LIST; i++)
      {
        leaf.AddPass(0, 0, new CellPass
        {
          InternalSiteModelMachineIndex = 0,
          Time = BASE_TIME.AddSeconds(i * TIME_INCREMENT_SECONDS),
          Height = BASE_HEIGHT + i * HEIGHT_DECREMENT,
          PassType = PassType.Front
        });
      }

      var cellPasses = leaf.Cells.PassesData[0].PassesData.ExtractCellPasses(0, 0);
      cellPasses.Length.Should().Be(PASSES_IN_DECREMENTING_ELEVATION_LIST);

      // Assign global latest cell pass to the appropriate pass
      leaf.Directory.GlobalLatestCells[0, 0] = cellPasses.Last();

      // Ensure all cell passes register the correct elevation mapping mode
      for (int i = 0; i < cellPasses.Length; i++)
        siteModel.MachinesTargetValues[0].ElevationMappingModeStateEvents.GetValueAtDate(cellPasses[i].Time, out _).Should().Be(ElevationMappingMode.MinimumElevation);

      // Ensure the pass data existence map records the existence of a non null value in the cell
      leaf.Directory.GlobalLatestCells.PassDataExistenceMap[0, 0] = true;

      // Count the number of non-null elevation cells to verify a correct setup
      long totalCells = 0;
      siteModel.Grid.Root.ScanSubGrids(siteModel.Grid.FullCellExtent(), x => {
        totalCells += leaf.Directory.GlobalLatestCells.PassDataExistenceMap.CountBits();
        return true;
      });
      totalCells.Should().Be(1);

      return siteModel;
    }

    private ISubGridRequestor[] CreateRequestorsForSingleCellTesting(ISiteModel siteModel,ICombinedFilter[] filters)
    {
      // Construct the set of requestors to query elevation sub grids needed for the summary volume calculations.
      var utilities = DIContext.Obtain<IRequestorUtilities>();
      var Requestors = utilities.ConstructRequestors(siteModel,
        utilities.ConstructRequestorIntermediaries(siteModel, new FilterSet(filters), true, GridDataType.HeightAndTime),
        AreaControlSet.CreateAreaControlSet(), siteModel.ExistenceMap);

      Requestors.Should().NotBeNull();
      Requestors.Length.Should().Be(1);

      return Requestors;
    }

    private IEnumerable<T> RequestAllSubGridsForSingleCellTesting<T>(ISiteModel siteModel, ISubGridRequestor[] requestors, GridDataType gridDataType)
    {
      // Request all elevation sub grids from the model
      var requestedSubGrids = new List<IClientLeafSubGrid>();
      siteModel.ExistenceMap.ScanAllSetBitsAsSubGridAddresses(x =>
      {
        IClientLeafSubGrid clientGrid = DIContext.Obtain<IClientLeafSubGridFactory>().GetSubGrid(gridDataType);
        if (requestors[0].RequestSubGridInternal(x, true, false, clientGrid) == ServerRequestResult.NoError)
          requestedSubGrids.Add(clientGrid);
      });

      return requestedSubGrids.Cast<T>();
    }

    [Fact]
    public void Test_ElevationSubgridRequests_SingleCell_SiteModelCreation()
    {
      var siteModel = CreateSiteModelWithSingleCellForTesting();

      // Construct the set of requestors to query elevation sub grids needed for the summary volume calculations.
      var Requestors = CreateRequestorsForSingleCellTesting(siteModel, new[] { new CombinedFilter() });

      // Request all elevation sub grids from the model
      var requestedSubGrids = RequestAllSubGridsForSingleCellTesting<IClientHeightLeafSubGrid>(siteModel, Requestors, GridDataType.Height);

      // Check exactly one-nonnull cell is present
      requestedSubGrids.Sum(x => x.CountNonNullCells()).Should().Be(1);
    }

    [Fact]
    public void Test_ElevationSubgridRequests_RequestElevationSubGrids_SingleCell_QueryWithNoFilter()
    {
      var siteModel = CreateSiteModelWithSingleCellForTesting();
      var requestors = CreateRequestorsForSingleCellTesting(siteModel, new[] {new CombinedFilter()});
      var subGrid = RequestAllSubGridsForSingleCellTesting<IClientHeightLeafSubGrid>(siteModel, requestors, GridDataType.Height).First();

      // Check cell has most recent height selected
      // Assumption: Absence of elevation mode filtering will ignore elevation mapping mode (may change)
      // --> Cell pass providing elevation is the most recent in time and hence lowest
      subGrid.Cells[0, 0].Should().Be(MINIMUM_HEIGHT);
    }

    [Fact]
    public void Test_ElevationSubGridRequests_RequestElevationSubGrids_SingleCell_QueryWithAsAtFilter_IncludesOnlyFirstPass()
    {
      var siteModel = CreateSiteModelWithSingleCellForTesting();

      var filter = new CombinedFilter();
      filter.AttributeFilter.SetHasTimeFilter(true);
      filter.AttributeFilter.EndTime = BASE_TIME;

      var requestors = CreateRequestorsForSingleCellTesting(siteModel, new[] {filter});

      var subGrid = RequestAllSubGridsForSingleCellTesting<IClientHeightLeafSubGrid>(siteModel, requestors, GridDataType.Height).First();

      // Check cell has has first height selected
      // Assumption: Elevation mode filtering has no impact on this scenario
      // --> Cell pass providing elevation is the earliest in time and hence highest
      subGrid.Cells[0, 0].Should().Be(MAXIMUM_HEIGHT);
    }

    [Fact]
    public void Test_ElevationSubGridRequests_RequestElevationSubGrids_SingleCell_QueryWithTimeRangeFilter_IncludesOnlySecondPass()
    {
      var siteModel = CreateSiteModelWithSingleCellForTesting();

      // Create a time range filter than bounds he time of the second added cell pass by 1 second before and after
      var filter = new CombinedFilter();
      filter.AttributeFilter.SetHasTimeFilter(true);
      filter.AttributeFilter.StartTime = BASE_TIME.AddSeconds(TIME_INCREMENT_SECONDS).AddSeconds(-1);
      filter.AttributeFilter.EndTime = BASE_TIME.AddSeconds(TIME_INCREMENT_SECONDS).AddSeconds(1);

      var requestors = CreateRequestorsForSingleCellTesting(siteModel, new[] { filter });

      var subGridHeight = RequestAllSubGridsForSingleCellTesting<IClientHeightLeafSubGrid>(siteModel, requestors, GridDataType.Height).First();

      // Check cell has has second height selected
      // Assumption: Elevation mode filtering has no impact on this scenario
      subGridHeight.Cells[0, 0].Should().Be(MAXIMUM_HEIGHT + HEIGHT_DECREMENT);

      var subGridPassCount = RequestAllSubGridsForSingleCellTesting<IClientPassCountLeafSubGrid>(siteModel, requestors, GridDataType.PassCount).First();

      // Check only a single cell was selected as a result of the time range filter
      subGridPassCount.Cells[0, 0].MeasuredPassCount.Should().Be(1);
    }
  }
}
