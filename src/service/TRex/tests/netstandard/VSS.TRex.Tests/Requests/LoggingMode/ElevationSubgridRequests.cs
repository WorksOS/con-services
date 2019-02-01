using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using VSS.TRex.Common;
using VSS.TRex.Common.Types;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees.Client.Interfaces;
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

    [Fact]
    public void Test_ElevationSubGridRequests_ModelConstruction()
    {
      var siteModel = Utilities.ConstructModelForTestsWithTwoExcavatorMachineTAGFiles(out var processedTasks);
      siteModel.Should().NotBeNull();
    }

    [Fact]
    public void Test_ElevationSubGridRequests_RequestElevationSubGrids_NoSurveyedSurfaces_NoFilter()
    {
      var siteModel = Utilities.ConstructModelForTestsWithTwoExcavatorMachineTAGFiles(out var processedTasks);

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
      var siteModel = Utilities.CreateSiteModelWithSingleCellWithMinimElevationPasses(BASE_TIME, TIME_INCREMENT_SECONDS, BASE_HEIGHT, HEIGHT_DECREMENT, PASSES_IN_DECREMENTING_ELEVATION_LIST);

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
      var siteModel = Utilities.CreateSiteModelWithSingleCellWithMinimElevationPasses(BASE_TIME, TIME_INCREMENT_SECONDS, BASE_HEIGHT, HEIGHT_DECREMENT, PASSES_IN_DECREMENTING_ELEVATION_LIST);
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
      var siteModel = Utilities.CreateSiteModelWithSingleCellWithMinimElevationPasses(BASE_TIME, TIME_INCREMENT_SECONDS, BASE_HEIGHT, HEIGHT_DECREMENT, PASSES_IN_DECREMENTING_ELEVATION_LIST);

      var filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasTimeFilter = true;
        x.AttributeFilter.EndTime = BASE_TIME;
      });

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
      var siteModel = Utilities.CreateSiteModelWithSingleCellWithMinimElevationPasses(BASE_TIME, TIME_INCREMENT_SECONDS, BASE_HEIGHT, HEIGHT_DECREMENT, PASSES_IN_DECREMENTING_ELEVATION_LIST);

      // Create a time range filter than bounds he time of the second added cell pass by 1 second before and after

      var filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasTimeFilter = true;
        x.AttributeFilter.StartTime = BASE_TIME.AddSeconds(TIME_INCREMENT_SECONDS).AddSeconds(-1);
        x.AttributeFilter.EndTime = BASE_TIME.AddSeconds(TIME_INCREMENT_SECONDS).AddSeconds(1);
      });

      var requestors = CreateRequestorsForSingleCellTesting(siteModel, new[] { filter });
      var subGridHeight = RequestAllSubGridsForSingleCellTesting<IClientHeightLeafSubGrid>(siteModel, requestors, GridDataType.Height).First();

      // Check cell has has second height selected
      // Assumption: Elevation mode filtering has no impact on this scenario
      subGridHeight.Cells[0, 0].Should().Be(MAXIMUM_HEIGHT + HEIGHT_DECREMENT);

      var subGridPassCount = RequestAllSubGridsForSingleCellTesting<IClientPassCountLeafSubGrid>(siteModel, requestors, GridDataType.PassCount).First();

      // Check only a single cell was selected as a result of the time range filter
      subGridPassCount.Cells[0, 0].MeasuredPassCount.Should().Be(1);
    }

    [Fact]
    public void Test_ElevationSubGridRequests_SingleCell_QueryWithElevationMappingModeFilter_LastPassOnly()
    {
      var siteModel = Utilities.CreateSiteModelWithSingleCellWithMinimElevationPasses(BASE_TIME, TIME_INCREMENT_SECONDS, BASE_HEIGHT, HEIGHT_DECREMENT, PASSES_IN_DECREMENTING_ELEVATION_LIST);

      var filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasElevationMappingModeFilter = true;
        x.AttributeFilter.ElevationMappingMode = ElevationMappingMode.LatestElevation;
      });

      var requestors = CreateRequestorsForSingleCellTesting(siteModel, new[] { filter });
      var subGridHeight = RequestAllSubGridsForSingleCellTesting<IClientHeightLeafSubGrid>(siteModel, requestors, GridDataType.Height).First();

      // Check cell has no height selected as no cell pass matches LastPassElevation mode
      subGridHeight.Cells[0, 0].Should().Be(Consts.NullHeight);
    }

    [Fact]
    public void Test_ElevationSubGridRequests_SingleCell_QueryWithElevationMappingModeFilter_MinimumElevationOnly()
    {
      var siteModel = Utilities.CreateSiteModelWithSingleCellWithMinimElevationPasses(BASE_TIME, TIME_INCREMENT_SECONDS, BASE_HEIGHT, HEIGHT_DECREMENT, PASSES_IN_DECREMENTING_ELEVATION_LIST);

      var filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasElevationMappingModeFilter = true;
        x.AttributeFilter.ElevationMappingMode = ElevationMappingMode.MinimumElevation;
      }); 

      var requestors = CreateRequestorsForSingleCellTesting(siteModel, new[] { filter });
      var subGridHeight = RequestAllSubGridsForSingleCellTesting<IClientHeightLeafSubGrid>(siteModel, requestors, GridDataType.Height).First();

      // Check cell has no height selected as no cell pass matches minimum elevation mode
      subGridHeight.Cells[0, 0].Should().Be(MINIMUM_HEIGHT);
    }

    [Fact()]
    public void Test_ElevationSubGridRequests_SingleCell_QueryWithMixedElevationMappingModes_NoFilter()
    {
      var siteModel = Utilities.CreateSiteModelWithSingleCellWithMixedElevationModePasses(BASE_TIME, TIME_INCREMENT_SECONDS, BASE_HEIGHT, HEIGHT_DECREMENT, PASSES_IN_DECREMENTING_ELEVATION_LIST);
      var requestors = CreateRequestorsForSingleCellTesting(siteModel, new[] { new CombinedFilter() });

      var subGridHeight = RequestAllSubGridsForSingleCellTesting<IClientHeightLeafSubGrid>(siteModel, requestors, GridDataType.Height).First();

      subGridHeight.Cells[0, 0].Should().Be(BASE_HEIGHT + 2 * HEIGHT_DECREMENT);
    }

    [Fact(Skip = "Not complete")]
    public void Test_ElevationSubGridRequests_SingleCell_QueryWithMixedElevationMappingModes_WithFilterOnMinimumElevationMode()
    {
      var siteModel = Utilities.CreateSiteModelWithSingleCellWithMixedElevationModePasses(BASE_TIME, TIME_INCREMENT_SECONDS, BASE_HEIGHT, HEIGHT_DECREMENT, PASSES_IN_DECREMENTING_ELEVATION_LIST);

      var filter = CombinedFilter.MakeFilterWith(x =>
      {
        x.AttributeFilter.HasElevationMappingModeFilter = true;
        x.AttributeFilter.ElevationMappingMode = ElevationMappingMode.MinimumElevation;
      });

      var requestors = CreateRequestorsForSingleCellTesting(siteModel, new[] { filter });
      var subGridHeight = RequestAllSubGridsForSingleCellTesting<IClientHeightLeafSubGrid>(siteModel, requestors, GridDataType.Height).First();

      // Until elevation mode behaviour implement, this should return the most recent minimum elevation pass
      subGridHeight.Cells[0, 0].Should().Be(BASE_HEIGHT);
    }

  }
}
