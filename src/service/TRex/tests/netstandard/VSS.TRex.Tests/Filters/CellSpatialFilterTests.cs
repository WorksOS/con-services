using System;
using FluentAssertions;
using VSS.TRex.Common;
using VSS.TRex.Geometry;
using Xunit;
using VSS.TRex.Filters;
using VSS.TRex.Tests.BinarizableSerialization;

namespace VSS.TRex.Tests.Filters
{
        public class CellSpatialFilterTests
    {
        [Fact()]
        public void Test_CellSpatialFilter_Creation()
        {
            CellSpatialFilter filter = new CellSpatialFilter();

            Assert.True(!filter.IsAlignmentMask && !filter.IsDesignMask && !filter.IsPositional && !filter.IsSpatial && !filter.IsSquare,
            "Cell spatial filter not created correctly");

            Assert.True(!filter.LeftOffset.HasValue && !filter.RightOffset.HasValue && 
                          !filter.StartStation.HasValue && !filter.EndStation.HasValue &&
                          filter.Fence.IsNull() && filter.AlignmentFence.IsNull() && 
                          filter.PositionX == Consts.NullDouble && filter.PositionY == Consts.NullDouble &&
                          filter.PositionRadius == Consts.NullDouble && filter.HasAlignmentDesignMask() == false &&
                          filter.HasSpatialOrPositionalFilters == false,// &&
//                          filter.OverrideSpatialCellRestriction == new BoundingIntegerExtent2D(),
            "Cell spatial filter not created correctly");
        }

        [Fact()]
        public void Test_CellSpatialFilter_ActiveFiltersString()
        {
            CellSpatialFilter filter = new CellSpatialFilter();

            Assert.Equal("Spatial:False, Positional:False, DesignMask:False, AlignmentMask:False", filter.ActiveFiltersString());
        }

        [Fact()]
        public void Test_CellSpatialFilter_Clear()
        {
            CellSpatialFilter filter = new CellSpatialFilter();

            filter.AlignmentFence.SetExtents(0, 0, 1, 1);
            filter.EndStation = 1000;
            filter.StartStation = 100;
            filter.LeftOffset = -1;
            filter.RightOffset = 1;
            filter.AlignmentDesignMaskDesignUID = Guid.Empty;
            filter.SurfaceDesignMaskDesignUid = Guid.Empty;
            filter.Fence.SetExtents(1, 1, 2, 2);
            filter.PositionRadius = 10;
            filter.PositionX = 10;
            filter.PositionY = 10;

            filter.Clear();

            Assert.True(filter.AlignmentFence.IsNull() &&
                          filter.Fence.IsNull() &&
                          !filter.EndStation.HasValue &&
                          !filter.StartStation.HasValue &&
                          !filter.LeftOffset.HasValue &&
                          !filter.RightOffset.HasValue &&
                          filter.AlignmentDesignMaskDesignUID == Guid.Empty &&
                          filter.SurfaceDesignMaskDesignUid == Guid.Empty &&
                          filter.PositionRadius == Consts.NullDouble &&
                          filter.PositionX == Consts.NullDouble &&
                          filter.PositionY == Consts.NullDouble,
                          "Filter.Clear did not clear all expected fields");
        }

        [Fact()]
        public void Test_CellSpatialFilter_SetAlignmentMask()
        {
            CellSpatialFilter filter = new CellSpatialFilter();

            filter.HasAlignmentDesignMask().Should().BeFalse();

            filter.IsAlignmentMask = true;
            filter.AlignmentFence = new Fence(0, 0, 1, 1);
            filter.AlignmentDesignMaskDesignUID = Guid.NewGuid();
            filter.StartStation = 100;
            filter.EndStation = 1000;
            filter.LeftOffset = -1;
            filter.RightOffset = 1;

            filter.HasAlignmentDesignMask().Should().BeTrue();

            Assert.True(filter.IsAlignmentMask && !filter.AlignmentFence.IsNull() &&
                filter.StartStation == 100 && filter.EndStation == 1000 && filter.LeftOffset == -1 && filter.RightOffset == 1 &&
                filter.AlignmentDesignMaskDesignUID != Guid.Empty,
                "Alignment mask not initialised correctly");
        }

        [Fact()]
        public void Test_CellSpatialFilter_SetDesignMask()
        {
            CellSpatialFilter filter = new CellSpatialFilter();

            filter.HasSurfaceDesignMask().Should().BeFalse();

            filter.IsDesignMask = true;
            filter.SurfaceDesignMaskDesignUid = Guid.NewGuid();

            Assert.True(filter.IsDesignMask && filter.SurfaceDesignMaskDesignUid != Guid.Empty, "Alignment mask not initialised correctly");
            filter.HasSurfaceDesignMask().Should().BeTrue();
        }

    [Fact()]
        public void Test_CellSpatialFilter_SetPositional()
        {
            CellSpatialFilter filter = new CellSpatialFilter();

            filter.IsPositional = true;
            filter.PositionX = 100;
            filter.PositionY = 200;
            filter.PositionRadius = 300;
            filter.IsSquare = true;

            Assert.True(filter.IsPositional && filter.PositionX == 100 && filter.PositionY == 200 && filter.PositionRadius == 300 && filter.IsSquare,
                "Positional filter not initialised correctly");
        }

        [Fact()]
        public void Test_CellSpatialFilter_SetSpatial()
        {
            CellSpatialFilter filter = new CellSpatialFilter();

            filter.IsSpatial = true;
            filter.Fence = new Fence(0, 0, 1, 1);

            Assert.True(filter.IsSpatial && filter.Fence.NumVertices == 4 && filter.Fence.Area() == 1,
                "Positional filter not initialised correctly");
        }

        [Fact()]
        public void Test_CellSpatialFilter_ClearAlignmentMask()
        {
            CellSpatialFilter filter = new CellSpatialFilter();

            filter.IsAlignmentMask = true;
            filter.AlignmentFence = new Fence(0, 0, 1, 1);

            filter.ClearAlignmentMask();
            Assert.True(!filter.IsAlignmentMask && filter.AlignmentFence.IsNull(), "Alignment mask not cleared correctly");
            filter.HasAlignmentDesignMask().Should().BeFalse();
        }

        [Fact()]
        public void Test_CellSpatialFilter_ClearDesignMask()
        {
            CellSpatialFilter filter = new CellSpatialFilter();

            filter.IsDesignMask = true;
            filter.SurfaceDesignMaskDesignUid = Guid.Empty;

            filter.ClearDesignMask();

            Assert.True(!filter.IsDesignMask && filter.SurfaceDesignMaskDesignUid == Guid.Empty,
                "Design mask not cleared correctly");
        }

        [Fact()]
        public void Test_CellSpatialFilter_ClearPositional()
        {
            CellSpatialFilter filter = new CellSpatialFilter();

            filter.IsPositional = true;
            filter.PositionX = 100;
            filter.PositionY = 200;
            filter.PositionRadius = 300;
            filter.IsSquare = true;

            filter.ClearPositional();

            Assert.True(!filter.IsPositional && filter.PositionX == Consts.NullDouble && filter.PositionY == Consts.NullDouble && filter.PositionRadius == Consts.NullDouble && !filter.IsSquare,
                "Positional filter not cleared correctly");
        }

        [Fact()]
        public void Test_CellSpatialFilter_ClearSpatial()
        {
            CellSpatialFilter filter = new CellSpatialFilter();

            filter.IsSpatial = true;
            filter.Fence = new Fence(0, 0, 1, 1);

            filter.ClearSpatial();

            Assert.True(!filter.IsSpatial && filter.Fence.IsNull(),  "Spatial filter not cleared correctly");
        }

        [Fact()]
        public void Test_CellSpatialFilter_IsCellInSelection_Polygon()
        {
            CellSpatialFilter filter = new CellSpatialFilter();
            filter.IsSpatial = true;
            filter.Fence = new Fence(0, 0, 1, 1);

            Assert.True(filter.IsCellInSelection(0.5, 0.5), "Cell location is IN the fence not OUT of it");
            Assert.False(filter.IsCellInSelection(10, 10), "Cell location is OUT of the fence not IN it");
        }

        [Fact()]
        public void Test_CellSpatialFilter_IsCellInSelection_Positional_PointRadiusCircular()
        {
            CellSpatialFilter filter = new CellSpatialFilter();
            filter.IsPositional = true;
            filter.PositionX = 0.0;
            filter.PositionY = 0.0;
            filter.PositionRadius = 1;
            filter.IsSquare = false;

            Assert.True(filter.IsCellInSelection(0.5, 0.5), "Cell location is IN the fence not OUT of it");
            Assert.True(filter.IsCellInSelection(0.0, 0.99999), "Cell location is IN the fence not OUT of it");
            Assert.True(filter.IsCellInSelection(0.0, -0.99999), "Cell location is IN the fence not OUT of it");
            Assert.True(filter.IsCellInSelection(0.99999, 0.0), "Cell location is IN the fence not OUT of it");
            Assert.True(filter.IsCellInSelection(-0.99999, 0.0), "Cell location is IN the fence not OUT of it");
            Assert.False(filter.IsCellInSelection(10, 10), "Cell location is OUT of the fence not IN it");
        }

        [Fact()]
        public void Test_CellSpatialFilter_IsCellInSelection_Positional_PointRadiusSquare()
        {
            CellSpatialFilter filter = new CellSpatialFilter();
            filter.IsPositional = true;
            filter.PositionX = 0.0;
            filter.PositionY = 0.0;
            filter.PositionRadius = 1.0;
            filter.IsSquare = true;

            Assert.True(filter.IsCellInSelection(0.5, 0.5), "Cell location is IN the fence not OUT of it");
            Assert.True(filter.IsCellInSelection(0.99909, 0.99999), "Cell location is IN the fence not OUT of it");
            Assert.True(filter.IsCellInSelection(0.99909, -0.99999), "Cell location is IN the fence not OUT of it");
            Assert.True(filter.IsCellInSelection(0.99999, 0.99909), "Cell location is IN the fence not OUT of it");
            Assert.True(filter.IsCellInSelection(-0.99999, 0.99909), "Cell location is IN the fence not OUT of it");
            Assert.False(filter.IsCellInSelection(10, 10), "Cell location is OUT of the fence not IN it");
        }

        [Fact()]
        public void Test_CellSpatialFilter_IsPositionInSelection()
        {
            CellSpatialFilter filter = new CellSpatialFilter();
            filter.IsSpatial = true;
            filter.Fence = new Fence();
            filter.Fence.SetExtents(0, 0, 1, 1);

            Assert.True(filter.IsPositionInSelection(0.5, 0.5), "Cell location is IN the fence not OUT of it");
            Assert.False(filter.IsCellInSelection(10, 10), "Cell location is OUT of the fence not IN it");
        }

        [Fact]
        public void Test_CellSpatialFilter_CalculateIntersectionWithExtents_Spatial()
        {
          var data = new CellSpatialFilter
          {
            IsSpatial = true,
            Fence = new Fence(1, 1, 100, 100)
          };

          BoundingWorldExtent3D expectedExtent = new BoundingWorldExtent3D(1, 1, 100, 100);
          BoundingWorldExtent3D startingExtent = new BoundingWorldExtent3D(0, 0, 101, 101);

          data.CalculateIntersectionWithExtents(startingExtent);
          startingExtent.Should().BeEquivalentTo(expectedExtent);
        }

        [Fact]
        public void Test_CellSpatialFilter_CalculateIntersectionWithExtents_Positional()
        {
          var data = new CellSpatialFilter
          {
            IsPositional = true,
            PositionX = 100,
            PositionY = 100,
            PositionRadius = 50
          };

          BoundingWorldExtent3D expectedExtent = new BoundingWorldExtent3D(50, 50, 150, 150);
          BoundingWorldExtent3D startingExtent = new BoundingWorldExtent3D(0, 0, 1000, 1000);

          data.CalculateIntersectionWithExtents(startingExtent);
          startingExtent.Should().BeEquivalentTo(expectedExtent);
        }

        [Fact]
        public void Test_CellSpatialFilter_FromToBinary()
        {
          var data = new CellSpatialFilter
          {
            PositionX = 1,
            PositionY = 2,
            PositionRadius = 3,
            IsAlignmentMask = true,
            IsPositional = true,
            Fence = new Fence(0, 0, 100, 100),
            AlignmentFence = new Fence(0, 0, 100, 100),
            IsSpatial = true,
            AlignmentDesignMaskDesignUID = Guid.NewGuid(),
            CoordsAreGrid = true,
            EndStation = 10,
            IsDesignMask = true,
            IsSquare = true,
            LeftOffset = 11,
            OverrideSpatialCellRestriction = new BoundingIntegerExtent2D(1, 1, 10, 10),
            RightOffset = 12,
            StartStation = 13,
            SurfaceDesignMaskDesignUid = Guid.NewGuid()
          };

          TestBinarizable_ReaderWriterHelper.RoundTripSerialise(data);
        }
    }
}
