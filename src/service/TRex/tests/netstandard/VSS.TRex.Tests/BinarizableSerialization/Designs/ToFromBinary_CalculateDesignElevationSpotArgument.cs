using System;
using VSS.TRex.Designs.GridFabric.Arguments;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Designs
{
  public class ToFromBinary_CalculateDesignElevationSpotArgument
  {
    [Fact]
    public void Test_ToFromBinary_CalculateDesignElevationSpotArgument_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<CalculateDesignElevationSpotArgument>("Empty CalculateDesignElevationSpotArgument not same after round trip serialisation");
    }

    [Fact]
    public void Test_CalculateDesignElevationSpotArgument()
    {
      var argument = new CalculateDesignElevationSpotArgument
      {
        ProjectID = Guid.NewGuid(),
        SpotX = 123.4,
        SpotY = 234.5,
        Filters = null,
        ReferenceDesign.DesignID = Guid.NewGuid(),
        ReferenceDesign.Offset = 999.9,
        TRexNodeID = "NodeID"        
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Custom CalculateDesignElevationSpotArgument not same after round trip serialisation");
    }
  }
}
