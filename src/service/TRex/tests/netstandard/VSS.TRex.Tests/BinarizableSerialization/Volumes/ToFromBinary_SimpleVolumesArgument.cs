using System;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Types;
using VSS.TRex.Volumes;
using VSS.TRex.Volumes.GridFabric.Arguments;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Volumes
{
  public class ToFromBinary_SimpleVolumesArgument
  {
    private const double CUT_TOLERANCE = 0.01;
    private const double FILL_TOLERANCE = 0.05;

    [Fact]
    public void Test_SimpleVolumesRequestArgument_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<SimpleVolumesRequestArgument>("Empty SimpleVolumesRequestArgument not same after round trip serialisation");
    }

    [Fact]
    public void Test_SimpleVolumesRequestArgument()
    {
      ICombinedFilter FromFilter = new CombinedFilter
      {
        AttributeFilter = new CellPassAttributeFilter
        {
          ReturnEarliestFilteredCellPass = true,
          HasElevationTypeFilter = true,
          ElevationType = ElevationType.First
        },

        SpatialFilter = new CellSpatialFilter
          {
            CoordsAreGrid = true,
            IsSpatial = true,
            Fence = new Fence(BoundingWorldExtent3D.Inverted())
          }
      };

      CombinedFilter ToFilter = new CombinedFilter()
      {
        AttributeFilter = new CellPassAttributeFilter()
        {
          ReturnEarliestFilteredCellPass = false,
          HasElevationTypeFilter = true,
          ElevationType = ElevationType.Last
        },

        SpatialFilter = FromFilter.SpatialFilter
      };
      var argument = new SimpleVolumesRequestArgument()
      {
        ProjectID = Guid.NewGuid(),
        VolumeType = VolumeComputationType.Between2Filters,
        BaseFilter = FromFilter,
        TopFilter = ToFilter,
        BaseDesignID = Guid.NewGuid(),
        TopDesignID = Guid.NewGuid(),
        CutTolerance = CUT_TOLERANCE,
        FillTolerance = FILL_TOLERANCE
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Custom SimpleVolumesRequestArgument not same after round trip serialisation");
    }
  }
}
