using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Types;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.Reports.StationOffset.Executors;
using VSS.TRex.Reports.StationOffset.GridFabric.Arguments;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Reports.StationOffset
{
  public class StationOffsetClusterComputeTests : IClassFixture<DITAGFileAndSubGridRequestsFixture>
  {
    [Fact]
    public void CalculateFromTAGFileDerivedModel()
    {
      var tagFiles = Directory.GetFiles(Path.Combine("TestData", "TAGFiles", "ElevationMappingMode-KettlewellDrive"), "*.tag").ToArray();
      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out var processedTasks);

      // For test purposes, create an imaginary 'road' that passes through at least 100 of the
      //  on-null cells in the site model, which also have passCount data
      var points = new List<StationOffsetPoint>();
      double station = 0;
      siteModel.Grid.Root.ScanSubGrids(siteModel.Grid.FullCellExtent(),
        subGrid =>
        {
          subGrid.CalculateWorldOrigin(out var originX, out var originY);

          ((IServerLeafSubGrid) subGrid).Directory.GlobalLatestCells.PassDataExistenceMap.ForEachSetBit(
            (x, y) =>
            {
              points.Add(new StationOffsetPoint(station += 1, 0,
                originY + y * siteModel.Grid.CellSize + siteModel.Grid.CellSize / 2,
                originX + x * siteModel.Grid.CellSize + siteModel.Grid.CellSize / 2));
            });

          return points.Count < 100;
        });

      var executor = new ComputeStationOffsetReportExecutor_ClusterCompute
      (new StationOffsetReportRequestArgument_ClusterCompute
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter()),
        Points = points,
        ReportElevation = true,
        ReportCmv = true,
        ReportMdp = true,
        ReportPassCount = true,
        ReportTemperature = true
      });

      var result = executor.Execute();

      result.ResultStatus.Should().Be(RequestErrorStatus.OK);
      result.ReturnCode.Should().Be(ReportReturnCode.NoError);
      result.StationOffsetRows.Count.Should().Be(points.Count);
      result.StationOffsetRows[0].Northing.Should().Be(808525.44000000006);
      result.StationOffsetRows[0].Easting.Should().Be(376730.88);
      result.StationOffsetRows[0].Elevation.Should().Be(68.6305160522461);
      result.StationOffsetRows[0].CutFill.Should().Be(Consts.NullHeight);
      result.StationOffsetRows[0].Cmv.Should().Be(CellPassConsts.NullCCV);
      result.StationOffsetRows[0].Mdp.Should().Be(CellPassConsts.NullMDP);
      result.StationOffsetRows[0].PassCount.Should().Be(1);
      result.StationOffsetRows[0].Temperature.Should().Be((short)CellPassConsts.NullMaterialTemperatureValue);

      result.StationOffsetRows.FirstOrDefault(x => x.CutFill != Consts.NullHeight).Should().Be(null);
      result.StationOffsetRows.FirstOrDefault(x => x.Cmv != CellPassConsts.NullCCV).Should().Be(null);
      result.StationOffsetRows.FirstOrDefault(x => x.Mdp != CellPassConsts.NullMDP).Should().Be(null);
      result.StationOffsetRows.FirstOrDefault(x => x.Temperature != (short)CellPassConsts.NullMaterialTemperatureValue).Should().Be(null);
    }

    [Fact]
    public void CalculateFromTAGFileDerivedModel_NoPoints()
    {
      var tagFiles = Directory.GetFiles(Path.Combine("TestData", "TAGFiles", "ElevationMappingMode-KettlewellDrive"), "*.tag").ToArray();
      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out var processedTasks);

      // Ask for a point that does not exist in the model the response should be a row with null values (???)
      var executor = new ComputeStationOffsetReportExecutor_ClusterCompute
      (new StationOffsetReportRequestArgument_ClusterCompute
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter()),
        Points = new List<StationOffsetPoint>(),
        ReportElevation = true
      });

      var result = executor.Execute();

      result.ResultStatus.Should().Be(RequestErrorStatus.OK);
      result.ReturnCode.Should().Be(ReportReturnCode.NoData);
      result.StationOffsetRows.Count.Should().Be(0);
    }

    [Fact]
    public void CalculateFromTAGFileDerivedModel_ShouldHaveNoPointValues()
    {
      var tagFiles = Directory.GetFiles(Path.Combine("TestData", "TAGFiles", "ElevationMappingMode-KettlewellDrive"), "*.tag").ToArray();
      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out var processedTasks);

      // Ask for a point that does not exist in the model the response should be a row with null values (???)
      var executor = new ComputeStationOffsetReportExecutor_ClusterCompute
      (new StationOffsetReportRequestArgument_ClusterCompute
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter()),
        Points = new List<StationOffsetPoint> { new StationOffsetPoint(0, 0, 0, 0) },
        ReportElevation = true
      });

      var result = executor.Execute();

      result.ResultStatus.Should().Be(RequestErrorStatus.OK);
      result.ReturnCode.Should().Be(ReportReturnCode.NoError);
      result.StationOffsetRows.Count.Should().Be(1);
      result.StationOffsetRows[0].Northing.Should().Be(0);
      result.StationOffsetRows[0].Easting.Should().Be(0);
      result.StationOffsetRows[0].Elevation.Should().Be(Consts.NullHeight);
    }

    [Fact]
    public void CalculateFromManuallyGeneratedSubGrid()
    {
      var siteModel = CreateSiteModelWithSingleCellForTesting();
      
      var executor = new ComputeStationOffsetReportExecutor_ClusterCompute
      (new StationOffsetReportRequestArgument_ClusterCompute
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter()),
        Points = new List<StationOffsetPoint> { new StationOffsetPoint(0, 0, 0, 0) },
        ReportElevation = true,
        ReportCmv = true,
        ReportMdp = true,
        ReportPassCount = true,
        ReportTemperature = true
      });

      var result = executor.Execute();

      result.ResultStatus.Should().Be(RequestErrorStatus.OK);
      result.ReturnCode.Should().Be(ReportReturnCode.NoError);
      result.StationOffsetRows.Count.Should().Be(1);
      result.StationOffsetRows[0].Northing.Should().Be(0);
      result.StationOffsetRows[0].Easting.Should().Be(0);
      result.StationOffsetRows[0].Elevation.Should().Be(MINIMUM_HEIGHT);
      result.StationOffsetRows[0].CutFill.Should().Be(Consts.NullHeight);
      result.StationOffsetRows[0].Cmv.Should().Be(CCV_Test);
      result.StationOffsetRows[0].Mdp.Should().Be(MDP_Test);
      result.StationOffsetRows[0].PassCount.Should().Be(3);
      result.StationOffsetRows[0].Temperature.Should().Be((short)Temperature_Test);
    }

    private readonly DateTime BASE_TIME = DateTime.Now;
    private const int TIME_INCREMENT_SECONDS = 10; // seconds
    private const float BASE_HEIGHT = 100.0f;
    private const float HEIGHT_DECREMENT = -0.1f;
    private const float MINIMUM_HEIGHT = BASE_HEIGHT + HEIGHT_DECREMENT * (PASSES_IN_DECREMENTING_ELEVATION_LIST - 1);

    private const int PASSES_IN_DECREMENTING_ELEVATION_LIST = 3;
    private const short CCV_Test = 34;
    private const short MDP_Test = 56;
    private const ushort Temperature_Test = 134;

    /// <summary>
    /// These private methods are copied from ElevationSubGridRequests tests
    /// </summary>
    /// <returns></returns>
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

      // vibrationState is needed to get cmv values
      siteModel.MachinesTargetValues[0].VibrationStateEvents.PutValueAtDate(BASE_TIME,VibrationState.On);
      siteModel.MachinesTargetValues[0].AutoVibrationStateEvents.PutValueAtDate(BASE_TIME, AutoVibrationState.Manual);

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
          PassType = PassType.Front,
          CCV = CCV_Test,
          MDP = MDP_Test,
          MaterialTemperature = Temperature_Test
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
  }
}
