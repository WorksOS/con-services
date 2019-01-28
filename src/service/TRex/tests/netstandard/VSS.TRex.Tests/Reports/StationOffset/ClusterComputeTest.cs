using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Remotion.Linq.Clauses.ResultOperators;
using VSS.TRex.Common.Types;
using VSS.TRex.Filters;
using VSS.TRex.Reports.StationOffset.Executors;
using VSS.TRex.Reports.StationOffset.GridFabric.Arguments;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Reports.StationOffset
{
  public class ClusterComputeTest : IClassFixture<DITAGFileAndSubGridRequestsFixture>
  {
    [Fact]
    public void Test_CalculationFromTAGFileDerivedModel()
    {
      var tagFiles = Directory.GetFiles(Path.Combine("TestData", "TAGFiles", "ElevationMappingMode-KettlewellDrive"), "*.tag").ToArray();
      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out var processedTasks);

      //var count = siteModel.Grid.CountLeafSubgridsInMemory();
      //siteModel.SiteModelExtent.Area.Should().Be(10);

      // For test purposes, create an imaginary 'road' that passes through at least 100 of the non-null cells in the site model
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
        ReportElevation = true
      });

      var result = executor.Execute();

      result.ResultStatus.Should().Be(RequestErrorStatus.OK);
      result.StationOffsetRows.Count.Should().Be(points.Count);
    }
  }
}
