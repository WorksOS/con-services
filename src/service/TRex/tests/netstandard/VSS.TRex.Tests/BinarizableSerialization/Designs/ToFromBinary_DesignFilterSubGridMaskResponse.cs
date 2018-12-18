using FluentAssertions;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Designs
{
  public class ToFromBinary_DesignFilterSubGridMaskResponse
  {
    [Fact]
    public void Test_DesignSubGridRequestArgumentBase_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<DesignFilterSubGridMaskResponse>("Empty DesignFilterSubGridMaskResponse not same after round trip serialisation");
    }

    [Fact]
    public void Test_DesignSubGridRequestArgumentBase_OneBit()
    {
      var argument = new DesignFilterSubGridMaskResponse();
      argument.Bits[10, 10] = true;

      var result = SimpleBinarizableInstanceTester.TestClass(argument, "Empty DesignFilterSubGridMaskResponse not same after round trip serialisation");

      result.member.Bits[10, 10].Should().Be(true);
      result.member.Bits.CountBits().Should().Be(1);
    }

    [Fact]
    public void Test_DesignSubGridRequestArgumentBase_FullBits()
    {
      var argument = new DesignFilterSubGridMaskResponse
      {
         Bits = SubGridTreeBitmapSubGridBits.FullMask
      };

      var result = SimpleBinarizableInstanceTester.TestClass(argument, "Empty DesignFilterSubGridMaskResponse not same after round trip serialisation");

      result.member.Bits.IsFull().Should().Be(true);
      result.member.Bits.CountBits().Should().Be(SubGridTreeConsts.CellsPerSubgrid);
    }
  }
}
