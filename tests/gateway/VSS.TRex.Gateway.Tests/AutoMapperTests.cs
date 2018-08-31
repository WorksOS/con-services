using System.Collections.Generic;
using Xunit;
using VSS.Productivity3D.Models.Models;
using Prod3d = VSS.Productivity3D.Models.Models;
using VSS.MasterData.Models.Models;
using VSS.TRex.Geometry;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;

namespace VSS.TRex.Gateway.Tests
{ 
  public class AutoMapperTests : IClassFixture<AutoMapperFixture>
  {
    [Fact]
    public void MapPointToFencePoint()
    {
      var point = new Point
      {
        x = 10,
        y = 15
      };
      var fencePoint = AutoMapperUtility.Automapper.Map<FencePoint>(point);
      Assert.Equal(point.x, fencePoint.X);
      Assert.Equal(point.y, fencePoint.Y);
      Assert.Equal(0, fencePoint.Z);
    }

    [Fact]
    public void MapWGSPoint3DToFencePoint()
    {
      var point = new WGSPoint3D(123.4, 567.8);
      var fencePoint = AutoMapperUtility.Automapper.Map<FencePoint>(point);
      Assert.Equal(point.Lon, fencePoint.X);
      Assert.Equal(point.Lat, fencePoint.Y);
      Assert.Equal(0, fencePoint.Z);
    }

    [Fact]
    public void MapBoundingBox2DGridToBoundingWorldExtent3D()
    {
      var box = new BoundingBox2DGrid(10, 12, 35, 27);  
      var box3d = AutoMapperUtility.Automapper.Map<BoundingWorldExtent3D>(box);
      Assert.Equal(box.bottomLeftX, box3d.MinX);
      Assert.Equal(box.bottomleftY, box3d.MinY);
      Assert.Equal(box.topRightX, box3d.MaxX);
      Assert.Equal(box.topRightY, box3d.MaxY);
    }

    [Fact]
    public void MapBoundingBox2DLatLonToBoundingWorldExtent3D()
    {
      var box = new BoundingBox2DLatLon(10, 12, 35, 27);
      var box3d = AutoMapperUtility.Automapper.Map<BoundingWorldExtent3D>(box);
      Assert.Equal(box.bottomLeftLon, box3d.MinX);
      Assert.Equal(box.bottomLeftLat, box3d.MinY);
      Assert.Equal(box.topRightLon, box3d.MaxX);
      Assert.Equal(box.topRightLat, box3d.MaxY);
    }

    [Fact]
    public void MapFilterResultWithPolygonToCombinedFilter()
    {
      List<WGSPoint3D> polygonLonLat = new List<WGSPoint3D>
      {
        new WGSPoint3D(1, 1),
        new WGSPoint3D(2, 2),
        new WGSPoint3D(3, 3)
      };
      var filter = new FilterResult(new Filter(), polygonLonLat, null, null, null, true, null);
      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filter);
      Assert.NotNull(combinedFilter.AttributeFilter);
      Assert.Equal(filter.ReturnEarliest, combinedFilter.AttributeFilter.ReturnEarliestFilteredCellPass);
      Assert.True(combinedFilter.AttributeFilter.HasElevationTypeFilter);
      Assert.Equal(Types.ElevationType.First, combinedFilter.AttributeFilter.ElevationType);
      Assert.Null(combinedFilter.AttributeFilter.SurveyedSurfaceExclusionList);
      Assert.NotNull(combinedFilter.SpatialFilter);
      Assert.False(combinedFilter.SpatialFilter.CoordsAreGrid);
      Assert.True(combinedFilter.SpatialFilter.IsSpatial);
      Assert.NotNull(combinedFilter.SpatialFilter.Fence);
      Assert.NotNull(combinedFilter.SpatialFilter.Fence.Points);
      Assert.Equal(polygonLonLat.Count, combinedFilter.SpatialFilter.Fence.Points.Count);
      for (int i =0; i<combinedFilter.SpatialFilter.Fence.Points.Count; i++)
      {
        Assert.Equal(filter.PolygonLL[i].Lon, combinedFilter.SpatialFilter.Fence.Points[i].X);
        Assert.Equal(filter.PolygonLL[i].Lat, combinedFilter.SpatialFilter.Fence.Points[i].Y);
      }
    }

    [Fact]
    public void MapFilterResultNoPolygonToCombinedFilter()
    {
      var filter = new FilterResult(new Filter(), null, null, null, null, true, null);
      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filter);
      Assert.NotNull(combinedFilter.AttributeFilter);
      Assert.Equal(filter.ReturnEarliest, combinedFilter.AttributeFilter.ReturnEarliestFilteredCellPass);
      Assert.True(combinedFilter.AttributeFilter.HasElevationTypeFilter);
      Assert.Equal(Types.ElevationType.First, combinedFilter.AttributeFilter.ElevationType);
      Assert.Null(combinedFilter.AttributeFilter.SurveyedSurfaceExclusionList);
      Assert.NotNull(combinedFilter.SpatialFilter);
      Assert.False(combinedFilter.SpatialFilter.CoordsAreGrid);
      Assert.False(combinedFilter.SpatialFilter.IsSpatial);
      Assert.Null(combinedFilter.SpatialFilter.Fence);
    }
  }
}
