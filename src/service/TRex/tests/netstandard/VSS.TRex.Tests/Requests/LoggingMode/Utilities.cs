﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using VSS.MasterData.Models.Models;
using VSS.TRex.Cells;
using VSS.TRex.Common.Types;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Server.Interfaces;
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

      // Switch to mutable storage representation to allow creation of content in the site model
      siteModel.SetStorageRepresentationToSupply(StorageMutability.Mutable);

      _ = siteModel.Machines.CreateNew("Bulldozer", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      _ = siteModel.Machines.CreateNew("Excavator", "", MachineType.Excavator, DeviceTypeEnum.SNM940, false, Guid.NewGuid());

      return siteModel;
    }

    public static void AddElevationMappingModeEvents(ISiteModel siteModel, short internalMachineIndex, IEnumerable<(DateTime, ElevationMappingMode)> events)
    {
      siteModel.MachinesTargetValues[internalMachineIndex].ElevationMappingModeStateEvents.PutValuesAtDates(events);
      siteModel.MachinesTargetValues[internalMachineIndex].ElevationMappingModeStateEvents.Count().Should().Be(events.Count());

      int index = 0;
      foreach (var evt in events)
      {
        siteModel.MachinesTargetValues[internalMachineIndex].ElevationMappingModeStateEvents.GetStateAtIndex(index++, out var eventDate, out var eventState);
        eventState.Should().Be(evt.Item2);
      }

      siteModel.MachinesTargetValues[internalMachineIndex].ElevationMappingModeStateEvents.LastStateValue().Should().Be(events.Last().Item2);
    }

    public static ISiteModel CreateSiteModelWithSingleCellWithMinimumElevationPasses(DateTime baseTime, int timeIncrementSeconds, float baseHeight, float heightDecrement, int numPassesToCreate)
    {
      // Set up a model with a single sub grid with a single cell containing two cell passes and a single
      // elevation mapping event with a state of lowest elevation mapping
      // Create the site model and machine etc to aggregate the processed TAG file into

      ISiteModel siteModel = NewEmptyModel();

      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;
      var excavatorMachineIndex = siteModel.Machines.Locate("Excavator", false).InternalSiteModelMachineIndex;

      // Add the lowest pass elevation mapping event occurring after a last pass mapping event
      AddElevationMappingModeEvents(siteModel, bulldozerMachineIndex, new[] {
        (baseTime.AddSeconds(-1), ElevationMappingMode.LatestElevation)
      });

      AddElevationMappingModeEvents(siteModel, excavatorMachineIndex, new[] {
        (baseTime.AddSeconds(-1), ElevationMappingMode.LatestElevation),
        (baseTime, ElevationMappingMode.MinimumElevation)
      });

      var cellPasses = Enumerable.Range(0, numPassesToCreate).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = excavatorMachineIndex,
          Time = baseTime.AddSeconds(x * timeIncrementSeconds),
          Height = baseHeight + x * heightDecrement,
          PassType = PassType.Front
        });

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses(siteModel, 0, 0, cellPasses, 1, numPassesToCreate);

      // Ensure all cell passes register the correct elevation mapping mode
      foreach (var cellPass in cellPasses)
        siteModel.MachinesTargetValues[excavatorMachineIndex].ElevationMappingModeStateEvents.GetValueAtDate(cellPass.Time, out _, ElevationMappingMode.LatestElevation).Should().Be(ElevationMappingMode.MinimumElevation);

      return siteModel;
    }

    public static ISiteModel CreateSiteModelWithSingleCellWithMixedElevationModePasses(DateTime baseTime, int timeIncrementSeconds, float baseHeight, float heightDecrement, int numPassesToCreate)
    {
      // Set up a model with a single sub grid with a single cell containing two cell passes and a single
      // elevation mapping event with a state of lowest elevation mapping

      // Create the site model and machine etc
      ISiteModel siteModel = NewEmptyModel();

      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;
      var excavatorMachineIndex = siteModel.Machines.Locate("Excavator", false).InternalSiteModelMachineIndex;

      // Add elevation mode mapping events to define two periods of minimum elevation mapping separated by a latest pass elevation mapping mode
      AddElevationMappingModeEvents(siteModel, excavatorMachineIndex, new[] {
        (baseTime.AddSeconds(-1), ElevationMappingMode.LatestElevation),
        (baseTime, ElevationMappingMode.MinimumElevation),
        (baseTime.AddMinutes(1), ElevationMappingMode.LatestElevation),
        (baseTime.AddMinutes(2), ElevationMappingMode.MinimumElevation),
      });

      AddElevationMappingModeEvents(siteModel, bulldozerMachineIndex, new[] {
        (baseTime.AddSeconds(-1), ElevationMappingMode.LatestElevation)
      });

      // Add a set of passes in the first minute to capture minimum elevation mode cutting
      IEnumerable<CellPass> cellPasses = Enumerable.Range(0, numPassesToCreate).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = excavatorMachineIndex,
          Time = baseTime.AddSeconds(x * timeIncrementSeconds),
          Height = baseHeight + x * heightDecrement,
          PassType = PassType.Front
        });

      // Ensure all cell passes register the correct elevation mapping mode
      foreach (var cellPass in cellPasses)
        siteModel.MachinesTargetValues[excavatorMachineIndex].ElevationMappingModeStateEvents.GetValueAtDate(cellPass.Time, out _, ElevationMappingMode.LatestElevation).Should().Be(ElevationMappingMode.MinimumElevation);

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses(siteModel, 0, 0, cellPasses, 1, numPassesToCreate);

      // Add a set of passes in the second minute to capture elevation mode filling
      cellPasses = Enumerable.Range(0, numPassesToCreate).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = bulldozerMachineIndex,
          Time = baseTime.AddMinutes(1).AddSeconds(x * timeIncrementSeconds),
          Height = baseHeight + x * -heightDecrement,
          PassType = PassType.Front
        });

      // Ensure all cell passes register the correct elevation mapping mode
      foreach (var cellPass in cellPasses)
        siteModel.MachinesTargetValues[bulldozerMachineIndex].ElevationMappingModeStateEvents.GetValueAtDate(cellPass.Time, out _, ElevationMappingMode.LatestElevation).Should().Be(ElevationMappingMode.LatestElevation);

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses(siteModel, 0, 0, cellPasses, 1, 2 * numPassesToCreate);

      // Add a set of passes in the third minute to capture latest elevation mode cutting back through the original cut zone
      cellPasses = Enumerable.Range(0, numPassesToCreate).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = excavatorMachineIndex,
          Time = baseTime.AddMinutes(2).AddSeconds(x * timeIncrementSeconds),
          Height = baseHeight + x * heightDecrement,
          PassType = PassType.Front
        });

      // Ensure all cell passes register the correct elevation mapping mode
      foreach (var cellPass in cellPasses)
        siteModel.MachinesTargetValues[excavatorMachineIndex].ElevationMappingModeStateEvents.GetValueAtDate(cellPass.Time, out _, ElevationMappingMode.LatestElevation).Should().Be(ElevationMappingMode.MinimumElevation);

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses(siteModel, 0, 0, cellPasses, 1, 3 * numPassesToCreate);

      // Add a set of passes in the fourth minute to mimic side trimming of the hole while in minimum elevation mode.
      // Use half the height increment of these passes to discriminate them from the cutting passes created above,.
      cellPasses = Enumerable.Range(0, numPassesToCreate).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = excavatorMachineIndex,
          Time = baseTime.AddMinutes(3).AddSeconds(x * timeIncrementSeconds),
          Height = baseHeight + x * (heightDecrement / 2),
          PassType = PassType.Front
        });

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses(siteModel, 0, 0, cellPasses, 1, 4 * numPassesToCreate);

      // Ensure all cell passes register the correct elevation mapping mode
      foreach (var cellPass in cellPasses)
        siteModel.MachinesTargetValues[excavatorMachineIndex].ElevationMappingModeStateEvents.GetValueAtDate(cellPass.Time, out _, ElevationMappingMode.LatestElevation).Should().Be(ElevationMappingMode.MinimumElevation);

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

      // Verify the existence map has the correct number of sub grids
      siteModel.ExistenceMap.CountBits().Should().Be(expectedSubgrids);

      // Ensure there are two appropriate elevation mapping mode events
      siteModel.MachinesTargetValues[0].ElevationMappingModeStateEvents.Count().Should().Be(expectedEvents);
      siteModel.MachinesTargetValues[0].ElevationMappingModeStateEvents.GetStateAtIndex(0, out var eventDate, out var eventState);
      eventState.Should().Be(ElevationMappingMode.LatestElevation);
      siteModel.MachinesTargetValues[0].ElevationMappingModeStateEvents.LastStateValue().Should().Be(ElevationMappingMode.MinimumElevation);

      // Count the number of non-null elevation cells per the sub grid pass dta existence maps
      long totalCells = 0;

      siteModel.ExistenceMap.ScanAllSetBitsAsSubGridAddresses(addr =>
      {
        var subGrid = TRex.SubGridTrees.Server.Utilities.SubGridUtilities.LocateSubGridContaining(
          siteModel.PrimaryStorageProxy, siteModel.Grid,
          addr.X, addr.Y, siteModel.Grid.NumLevels, false, false);

       totalCells += ((IServerLeafSubGrid)subGrid).Directory.GlobalLatestCells.PassDataExistenceMap.CountBits();
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
