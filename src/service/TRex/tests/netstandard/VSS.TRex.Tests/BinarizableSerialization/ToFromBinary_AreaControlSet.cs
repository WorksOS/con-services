using System;
using FluentAssertions;
using VSS.TRex.Common.Types;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class ToFromBinary_AreaControlSet
  {
    [Fact]
    public void Test_AreaControlSet_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<SiteModelMetadata>("Empty AreaControlSet not same after round trip serialisation");
    }

    [Fact]
    public void Test_AreaControlSet()
    {
      var argument = new AreaControlSet(true, 1000, 999, 10, 99, 8);

      var result = SimpleBinarizableInstanceTester.TestClass(argument, "Custom AreaControlSet not same after round trip serialisation");

      argument.PixelXWorldSize.Should().Be(result.member.PixelXWorldSize, "YPixelWorldSize are not equal");
    }
  }
}
