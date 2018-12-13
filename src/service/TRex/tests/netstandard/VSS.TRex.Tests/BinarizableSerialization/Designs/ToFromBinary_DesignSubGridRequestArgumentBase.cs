using System;
using VSS.TRex.Designs.GridFabric.Arguments;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Designs
{
  public class ToFromBinary_DesignSubGridRequestArgumentBase
  {
    [Fact]
    public void Test_DesignSubGridRequestArgumentBase_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<DesignSubGridRequestArgumentBase>("Empty DesignSubGridRequestArgumentBase not same after round trip serialisation");
    }

    [Fact]
    public void Test_DesignSubGridRequestArgumentBase_SubgridDetail()
    {
      var argument = new DesignSubGridRequestArgumentBase()
      {
        ProjectID = Guid.NewGuid(),
        ReferenceDesignUID = Guid.Empty,
        CellSize = 1.0,
        OriginX = 123,
        OriginY = 456
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Empty DesignSubGridRequestArgumentBase not same after round trip serialisation");
    }
  }
}
