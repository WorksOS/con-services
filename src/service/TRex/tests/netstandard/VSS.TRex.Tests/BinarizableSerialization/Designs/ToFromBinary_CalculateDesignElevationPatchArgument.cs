using System;
using VSS.TRex.Designs.GridFabric.Arguments;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Designs
{
  public class ToFromBinary_CalculateDesignElevationPatchArgument
  {
    [Fact]
    public void Test_ToFromBinary_CalculateDesignElevationPatchArgument_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<CalculateDesignElevationPatchArgument>("Empty CalculateDesignElevationPatchArgument not same after round trip serialisation");
    }

    [Fact]
    public void Test_CalculateDesignElevationPatchArgument()
    {
      var argument = new CalculateDesignElevationPatchArgument
      {
        ProjectID = Guid.NewGuid(),
        CellSize = 0.34,
        OriginX = 1234,
        OriginY = 2345,
        Filters = null,
        ReferenceDesign.DesignID = Guid.NewGuid(),
        ReferenceDesign.Offset = 999.9,
        TRexNodeID = "NodeID"        
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Custom CalculateDesignElevationPatchArgument not same after round trip serialisation");
    }
  }
}
