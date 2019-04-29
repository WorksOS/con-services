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
      var argument = new DesignSubGridRequestArgumentBase
      {
        ProjectID = Guid.NewGuid(),
        ReferenceDesignUID = Guid.Empty,
        ReferenceOffset = 123.4,
        TRexNodeID = "NodeID",
        Filters = null
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Empty DesignSubGridRequestArgumentBase not same after round trip serialisation");
    }
  }
}
