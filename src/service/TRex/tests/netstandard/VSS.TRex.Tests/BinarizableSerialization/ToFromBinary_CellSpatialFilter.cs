using System;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class ToFromBinary_CellSpatialFilter
  {
    [Fact]
    public void Test_CellSpatialFilter_Simple()
    {
      SimpleBinarizableInstanceTester.TestClassEx<CellSpatialFilter>();
    }

    [Fact]
    public void Test_CellSpatialFilter_CoordsAreGrid()
    {
      SimpleBinarizableInstanceTester.TestClassEx<CellSpatialFilter>(new CellSpatialFilter
      {
        CoordsAreGrid = true
      },
      "Cell spatial filter (CoordsAreGrid) not the same after round trip serialisation");
    }

    [Fact]
    public void Test_CellSpatialFilter_IsSquare()
    {
      SimpleBinarizableInstanceTester.TestClassEx<CellSpatialFilter>(new CellSpatialFilter
        {
          IsSquare = true,
          Fence = new Fence(0, 0, 10, 10)
        },
        "Cell spatial filter (IsSquare) not the same after round trip serialisation");
    }

    [Fact]
    public void Test_CellSpatialFilter_IsAlignmentFence()
    {
      SimpleBinarizableInstanceTester.TestClassEx<CellSpatialFilter>(new CellSpatialFilter
        {
          IsAlignmentMask = true,
          StartStation = 0.0,
          EndStation = 123.0,
          LeftOffset = 5,
          RightOffset = 7.5,
          AlignmentFence = new Fence(0, 0, 10, 10),
          AlignmentDesignMaskDesignUID = Guid.NewGuid()
        },
        "Cell spatial filter (IsAlignmentFence) not the same after round trip serialisation");
    }

    [Fact]
    public void Test_CellSpatialFilter_IsPositional()
    {
      SimpleBinarizableInstanceTester.TestClassEx<CellSpatialFilter>(new CellSpatialFilter
        {
          PositionX = 10,
          PositionY = 11,
          PositionRadius = 123,
          IsPositional = true
        },
        "Cell spatial filter (IsPositional) not the same after round trip serialisation");
    }

    [Fact]
    public void Test_CellSpatialFilter_IsDesignMask()
    {
      SimpleBinarizableInstanceTester.TestClassEx<CellSpatialFilter>(new CellSpatialFilter
        {
          IsDesignMask = true,
          SurfaceDesignMaskDesignUid = Guid.NewGuid()
        },
        "Cell spatial filter (is IsDesignMask) not the same after round trip serialisation");
    }

    [Fact]
    public void Test_CellSpatialFilter_OverrideSpatialCellRestriction()
    {
      SimpleBinarizableInstanceTester.TestClassEx<CellSpatialFilter>(new CellSpatialFilter
        {
           OverrideSpatialCellRestriction = new BoundingIntegerExtent2D(1, 2, 3, 4)
        },
        "Cell spatial filter (OverrideSpatialCellRestriction) not the same after round trip serialisation");
    }

    [Fact]
    public void Test_CellSpatialFilter_IsSpatial()
    {
      SimpleBinarizableInstanceTester.TestClassEx<CellSpatialFilter>(new CellSpatialFilter
        {
          IsSpatial = true,
          Fence = new Fence(0, 0, 10, 10)
        },
        "Cell spatial filter (IsSpatial) not the same after round trip serialisation");
    }
  }
}
