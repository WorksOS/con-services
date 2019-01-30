using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using VSS.TRex.Cells;
using VSS.TRex.Common.Types;
using VSS.TRex.DI;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.TAGFiles.Classes.Integrator;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;

namespace VSS.TRex.Tests.Requests.LoggingMode
{
  public static class Utilities
  {
    public static ISiteModel NewEmptyModel()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      _ = siteModel.Machines.CreateNew("Test Machine", "", 1, 1, false, Guid.NewGuid());

      return siteModel;
    }

    public static void AddElevationMappingModeEvents(ISiteModel siteModel, IEnumerable<(DateTime, ElevationMappingMode)> events)
    {
      siteModel.MachinesTargetValues[0].ElevationMappingModeStateEvents.PutValuesAtDates(events);
      siteModel.MachinesTargetValues[0].ElevationMappingModeStateEvents.Count().Should().Be(events.Count());
    }

    public static void AddSingleCellAtOriginWithPasses(ISiteModel siteModel, IEnumerable<CellPass> passes)
    {
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

      CellPass[] _passes = passes.ToArray();

      foreach (var pass in _passes)
        leaf.AddPass(0, 0, pass);

      var cellPasses = leaf.Cells.PassesData[0].PassesData.ExtractCellPasses(0, 0);
      cellPasses.Length.Should().Be(_passes.Length);

      // Assign global latest cell pass to the appropriate pass
      leaf.Directory.GlobalLatestCells[0, 0] = cellPasses.Last();

      // Ensure the pass data existence map records the existence of a non null value in the cell
      leaf.Directory.GlobalLatestCells.PassDataExistenceMap[0, 0] = true;

      // Count the number of non-null elevation cells to verify a correct setup
      long totalCells = 0;
      siteModel.Grid.Root.ScanSubGrids(siteModel.Grid.FullCellExtent(), x => {
        totalCells += leaf.Directory.GlobalLatestCells.PassDataExistenceMap.CountBits();
        return true;
      });

      totalCells.Should().Be(1);
    }

    public static ISiteModel CreateSiteModelWithSingleCellForTesting(DateTime baseTime, int timeIncrementSeconds, float baseHeight, float heightDecrement, int numPassesToCreate)
    {
      // Set up a model with a single sub grid with a single cell containing two cell passes and a single
      // elevation mapping event with a state of lowest elevation mapping
      // Create the site model and machine etc to aggregate the processed TAG file into

      ISiteModel siteModel = NewEmptyModel();

      // Add the lowest pass elevation mapping event occurring after a last pass mapping event
      AddElevationMappingModeEvents(siteModel, new[] {
          (baseTime.AddSeconds(-1), ElevationMappingMode.LatestElevation),
          (baseTime, ElevationMappingMode.MinimumElevation)
        });

      siteModel.MachinesTargetValues[0].ElevationMappingModeStateEvents.GetStateAtIndex(0, out var eventDate, out var eventState);
      eventState.Should().Be(ElevationMappingMode.LatestElevation);
      siteModel.MachinesTargetValues[0].ElevationMappingModeStateEvents.LastStateValue().Should().Be(ElevationMappingMode.MinimumElevation);

      IEnumerable<CellPass> cellPasses = Enumerable.Range(0, numPassesToCreate).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = 0,
          Time = baseTime.AddSeconds(x * timeIncrementSeconds),
          Height = baseHeight + x * heightDecrement,
          PassType = PassType.Front
        });

      AddSingleCellAtOriginWithPasses(siteModel, cellPasses);

      // Ensure all cell passes register the correct elevation mapping mode
      foreach (var cellPass in cellPasses)
        siteModel.MachinesTargetValues[0].ElevationMappingModeStateEvents.GetValueAtDate(cellPass.Time, out _).Should().Be(ElevationMappingMode.MinimumElevation);

      return siteModel;
    }

    public static ISiteModel ConstructModelForTestsWithTwoExcavatorMachineTAGFiles(out List<AggregatedDataIntegratorTask> processedTasks)
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
          totalCells += ((IServerLeafSubGrid)subGrid).Directory.GlobalLatestCells.PassDataExistenceMap.CountBits();
          return true;
        });
      totalCells.Should().Be(expectedNonNullCells);

      siteModel.SiteModelExtent.MinX.Should().BeApproximately(376735.98, 0.001);
      siteModel.SiteModelExtent.MaxX.Should().BeApproximately(376742.78, 0.001);
      siteModel.SiteModelExtent.MinY.Should().BeApproximately(808534.28, 0.001);
      siteModel.SiteModelExtent.MaxY.Should().BeApproximately(808542.78, 0.001);
      siteModel.SiteModelExtent.MinZ.Should().BeApproximately(66.4441, 0.001);
      siteModel.SiteModelExtent.MaxZ.Should().BeApproximately(68.5629, 0.001);

      return siteModel;
    }
  }
}
