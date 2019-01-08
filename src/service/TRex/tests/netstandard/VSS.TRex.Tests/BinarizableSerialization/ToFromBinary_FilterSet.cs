using System.Collections.Generic;
using FluentAssertions;
using VSS.MasterData.Models.Models;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class ToFromBinary_CombinedFillToFromBinary_FilterSetter
  {
    [Fact]
    public void ToFromBinary_FilterSet_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<FilterSet>("Empty FilterSet not same after round trip serialisation");
    }

    [Fact]
    public void ToFromBinary_FilterSet_OneFilter()
    {
      SimpleBinarizableInstanceTester.TestClass<FilterSet>(new FilterSet(new CombinedFilter()),
        "FilterSet with one empty filter not same after round trip serialisation");
    }

    [Fact]
    public void ToFromBinary_FilterSet_TwoFilters()
    {
      SimpleBinarizableInstanceTester.TestClass<FilterSet>(new FilterSet(new CombinedFilter(), new CombinedFilter()),
        "FilterSet with one empty filter not same after round trip serialisation");
    }

    [Fact]
    public void ToFromBinary_FilterSet_FencePoints()
    {
      var fence = new Fence();
      fence.Points.Add(new FencePoint(1, 50));
      fence.Points.Add(new FencePoint(2, 51));
      fence.UpdateExtents();


      var filterSet = new FilterSet
      (new CombinedFilter
        (new CellPassAttributeFilter() { },
          new CellSpatialFilter()
          {
            Fence = fence,
            AlignmentFence = fence,
            PositionX = 34,
            PositionY = 56,
            PositionRadius = 4,
            IsSquare = false,
            CoordsAreGrid = true
          }
        )
      );

      var result = SimpleBinarizableInstanceTester.TestClass(filterSet,
        "FilterSet with one empty filter not same after round trip serialisation");
      filterSet.Filters.Length.Should().Be(1, "spatialFenceFilter count is wrong");
      filterSet.Filters[0].SpatialFilter.Fence.Points.Count
        .Should().Be(2, "spatialFenceYCoords count is wrong");
      filterSet.Filters[0].SpatialFilter.Fence[0].Y
        .Should().Be(result.member.Filters[0].SpatialFilter.Fence[0].Y,
          "spatialFenceYCoords are not equal");
    }

    [Fact]
    public void ToFromBinary_FilterSet_FencePoints2()
    {
      var fence = new Fence();
      fence.Points.Add(new FencePoint(1, 50));
      fence.Points.Add(new FencePoint(2, 51));
      fence.UpdateExtents();

      FilterSet filterSet = new FilterSet();

      filterSet.Filters = new ICombinedFilter[1];
      var cf =
        new CombinedFilter
        (new CellPassAttributeFilter() { },
          new CellSpatialFilter()
          {
            PositionX = 34,
            PositionY = 56,
            PositionRadius = 4,
            IsSquare = false,
            CoordsAreGrid = true
          }
        );
      cf.SpatialFilter.Fence = fence;

      filterSet.Filters[0] = cf;

      var result = SimpleBinarizableInstanceTester.TestClass(filterSet,
        "FilterSet with one empty filter not same after round trip serialisation");
      filterSet.Filters.Length.Should().Be(1, "spatialFenceFilter count is wrong");
      filterSet.Filters[0].SpatialFilter.Fence.Points.Count
        .Should().Be(2, "spatialFenceYCoords count is wrong");
      filterSet.Filters[0].SpatialFilter.Fence[0].Y
        .Should().Be(result.member.Filters[0].SpatialFilter.Fence[0].Y,
          "spatialFenceYCoords are not equal");
    }

  }
}

