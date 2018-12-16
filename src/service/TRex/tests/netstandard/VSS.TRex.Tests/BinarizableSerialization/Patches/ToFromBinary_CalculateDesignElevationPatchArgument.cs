using System;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Tests.BinarizableSerialization.Analytics;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Patches
{
  public class ToFromBinary_CalculateDesignElevationPatchArgument : BaseTests, IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_CalculateDesignElevationPatchArgument_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<CalculateDesignElevationPatchArgument>("Empty CalculateDesignElevationPatchArgument not same after round trip serialisation");
    }

    [Fact]
    public void Test_CalculateDesignElevationPatchArgument()
    {
      var argument = new CalculateDesignElevationPatchArgument()
      {
        ProjectID = Guid.NewGuid(),
        Filters = new FilterSet(new CombinedFilter()),
        CellSize = CELL_SIZE,
        ReferenceDesignUID = Guid.Empty,
        Offset = 0.0,
        OriginX = 12345,
        OriginY = 67890
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Custom CalculateDesignElevationPatchArgument not same after round trip serialisation");
    }
  }
}
