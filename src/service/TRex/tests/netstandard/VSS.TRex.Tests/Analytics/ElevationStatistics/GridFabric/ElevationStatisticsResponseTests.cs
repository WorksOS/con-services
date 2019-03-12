using System;
using VSS.TRex.Analytics.ElevationStatistics.GridFabric;
using VSS.TRex.Common;
using VSS.TRex.Geometry;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.ElevationStatistics.GridFabric
{
  public class ElevationStatisticsResponseTests : BaseTests
  {
    private const double MIN_ELEVATION = 123;
    private const double MAX_ELEVATION = 56789;
    private const int CELLS_USED = 1978;
    private const int CELLS_SCANNED = 2223;
    private const double MIN_X = 1234.56;
    private const double MIN_Y = 7890.12;
    private const double MIN_Z = 123.45;
    private const double MAX_X = 1294.78;
    private const double MAX_Y = 7940.67;
    private const double MAX_Z = 678.34;


    private ElevationStatisticsResponse _response => new ElevationStatisticsResponse()
    {
      ResultStatus = RequestErrorStatus.OK,
      CellSize = CELL_SIZE,
      MinElevation = MIN_ELEVATION,
      MaxElevation = MAX_ELEVATION,
      CellsUsed = CELLS_USED,
      CellsScanned = CELLS_SCANNED,
      BoundingExtents = new BoundingWorldExtent3D(MIN_X, MIN_Y, MAX_X, MAX_Y, MIN_Z, MAX_Z)
    };

    [Fact]
    public void Test_ElevationStatisticsResponse_Creation()
    {
      var response = new ElevationStatisticsResponse();

      Assert.True(response.ResultStatus == RequestErrorStatus.Unknown, "ResultStatus invalid after ElevationStatisticsResponse creation.");
      Assert.True(response.CellSize < Consts.TOLERANCE_DIMENSION, "CellSize invalid after ElevationStatisticsResponse creation.");
      Assert.True(Math.Abs(response.MinElevation) < Consts.TOLERANCE_HEIGHT, "Invalid initial value for MinElevation.");
      Assert.True(Math.Abs(response.MaxElevation) < Consts.TOLERANCE_HEIGHT, "Invalid initial value for MaxElevation.");
      Assert.True(response.CellsUsed == 0, "Invalid initial value for CellsUsed.");
      Assert.True(response.CellsScanned == 0, "Invalid initial value for CellsScanned.");
      Assert.True(Math.Abs(response.BoundingExtents.MinX) < Consts.TOLERANCE_DIMENSION, "Invalid initial value for BoundingExtents.MinX.");
      Assert.True(Math.Abs(response.BoundingExtents.MinY) < Consts.TOLERANCE_DIMENSION, "Invalid initial value for BoundingExtents.MinY.");
      Assert.True(Math.Abs(response.BoundingExtents.MinZ) < Consts.TOLERANCE_HEIGHT, "Invalid initial value for BoundingExtents.MinZ.");
      Assert.True(Math.Abs(response.BoundingExtents.MaxX) < Consts.TOLERANCE_DIMENSION, "Invalid initial value for BoundingExtents.MaxX.");
      Assert.True(Math.Abs(response.BoundingExtents.MaxY) < Consts.TOLERANCE_DIMENSION, "Invalid initial value for BoundingExtents.MaxY.");
      Assert.True(Math.Abs(response.BoundingExtents.MaxZ) < Consts.TOLERANCE_HEIGHT, "Invalid initial value for BoundingExtents.MaxZ.");
    }

    [Fact]
    public void Test_ElevationStatisticsResponse_ConstructResult_Successful()
    {
      Assert.True(_response.ResultStatus == RequestErrorStatus.OK, "Invalid initial result status");

      var result = _response.ConstructResult();

      Assert.True(result.ResultStatus == RequestErrorStatus.OK, "Result status invalid, not propagaged from aggregation state");

      Assert.True(Math.Abs(result.MinElevation - _response.MinElevation) < Consts.TOLERANCE_HEIGHT, "Invalid initial result value for MinElevation.");
      Assert.True(Math.Abs(result.MaxElevation - _response.MaxElevation) < Consts.TOLERANCE_HEIGHT, "Invalid initial result value for MaxElevation.");
      Assert.True(Math.Abs(result.CoverageArea - _response.CoverageArea) < Consts.TOLERANCE_AREA, "Invalid initial result value for CoverageArea.");
      Assert.True(Math.Abs(result.BoundingExtents.MinX - _response.BoundingExtents.MinX) < Consts.TOLERANCE_DIMENSION, "Invalid initial result value for BoundingExtents.MinX.");
      Assert.True(Math.Abs(result.BoundingExtents.MinY - _response.BoundingExtents.MinY) < Consts.TOLERANCE_DIMENSION, "Invalid initial result value for BoundingExtents.MinY.");
      Assert.True(Math.Abs(result.BoundingExtents.MinZ - _response.BoundingExtents.MinZ) < Consts.TOLERANCE_HEIGHT, "Invalid initial result value for BoundingExtents.MinZ.");
      Assert.True(Math.Abs(result.BoundingExtents.MaxX - _response.BoundingExtents.MaxX) < Consts.TOLERANCE_DIMENSION, "Invalid initial result value for BoundingExtents.MaxX.");
      Assert.True(Math.Abs(result.BoundingExtents.MaxY - _response.BoundingExtents.MaxY) < Consts.TOLERANCE_DIMENSION, "Invalid initial result value for BoundingExtents.MaxY.");
      Assert.True(Math.Abs(result.BoundingExtents.MaxZ - _response.BoundingExtents.MaxZ) < Consts.TOLERANCE_HEIGHT, "Invalid initial result value for BoundingExtents.MaxZ.");
    }

    [Fact]
    public void Test_ElevationStatisticsResponse_AgregateWith_Successful()
    {
      var responseClone = new ElevationStatisticsResponse()
      {
        ResultStatus = _response.ResultStatus,
        CellSize = _response.CellSize,
        MinElevation = _response.MinElevation,
        MaxElevation = _response.MaxElevation,
        CellsUsed = _response.CellsUsed,
        CellsScanned = _response.CellsScanned,
        BoundingExtents = _response.BoundingExtents
      };

      var response = _response.AggregateWith(responseClone);

      Assert.True(Math.Abs(response.CellSize - _response.CellSize) < Consts.TOLERANCE_DIMENSION, "CellSize invalid after aggregation.");
      Assert.True(Math.Abs(response.MinElevation - _response.MinElevation) < Consts.TOLERANCE_HEIGHT, "Invalid aggregated value for MinElevation.");
      Assert.True(Math.Abs(response.MaxElevation - _response.MaxElevation) < Consts.TOLERANCE_HEIGHT, "Invalid aggregated value for MaxElevation.");
      Assert.True(response.CellsUsed == _response.CellsUsed * 2, "Invalid aggregated value for CellsUsed.");
      Assert.True(response.CellsScanned == _response.CellsScanned * 2, "Invalid aggregated value for CellsScanned.");
      Assert.True(Math.Abs(response.BoundingExtents.MinX - _response.BoundingExtents.MinX) < Consts.TOLERANCE_DIMENSION, "Invalid aggregated value for BoundingExtents.MinX.");
      Assert.True(Math.Abs(response.BoundingExtents.MinY - _response.BoundingExtents.MinY) < Consts.TOLERANCE_DIMENSION, "Invalid aggregated value for BoundingExtents.MinY.");
      Assert.True(Math.Abs(response.BoundingExtents.MinZ - _response.BoundingExtents.MinZ) < Consts.TOLERANCE_HEIGHT, "Invalid aggregated value for BoundingExtents.MinZ.");
      Assert.True(Math.Abs(response.BoundingExtents.MaxX - _response.BoundingExtents.MaxX) < Consts.TOLERANCE_DIMENSION, "Invalid aggregated value for BoundingExtents.MaxX.");
      Assert.True(Math.Abs(response.BoundingExtents.MaxY - _response.BoundingExtents.MaxY) < Consts.TOLERANCE_DIMENSION, "Invalid aggregated value for BoundingExtents.MaxY.");
      Assert.True(Math.Abs(response.BoundingExtents.MaxZ - _response.BoundingExtents.MaxZ) < Consts.TOLERANCE_HEIGHT, "Invalid aggregated value for BoundingExtents.MaxZ.");
    }
  }
}
