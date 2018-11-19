using System;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Geometry;
//using VSS.TRex.Tests.BinarizableSerialization.Analytics;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Designs
{
  public class ToFromBinary_CalculateDesignProfileArgument //: IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_CalculateDesignElevationPatchArgument_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<CalculateDesignProfileArgument>("Empty CalculateDesignProfileArgument not same after round trip serialisation");
    }

    [Fact]
    public void Test_CalculateDesignElevationPatchArgument()
    {
      var argument = new CalculateDesignProfileArgument()
      {
        ProjectID = Guid.NewGuid(),
        DesignUid = Guid.Empty, 
        CellSize = 1.0,
        ProfilePath = new [] {new XYZ(0, 0), new XYZ(100, 100)}
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Custom CalculateDesignProfileArgument not same after round trip serialisation");
    }
  }
}
