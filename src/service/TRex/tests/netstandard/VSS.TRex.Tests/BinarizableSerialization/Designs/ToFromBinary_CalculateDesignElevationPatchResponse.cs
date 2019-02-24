using FluentAssertions;
using VSS.TRex.Common;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Core.Utilities;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Designs
{
  public class ToFromBinary_CalculateDesignElevationPatchResponse
  {
    [Fact]
    public void Test_ToFromBinary_CalculateDesignElevationPatchResponse_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<CalculateDesignElevationPatchResponse>("Empty CalculateDesignElevationPatchResponse not same after round trip serialisation");
    }

    [Fact]
    public void Test_CalculateDesignElevationPatchArgument_NullHeights()
    {
      var argument = new CalculateDesignElevationPatchResponse
      {
        CalcResult = DesignProfilerRequestResult.OK,
        Heights = new ClientHeightLeafSubGrid()
      };

      var result = SimpleBinarizableInstanceTester.TestClass(argument, "Custom CalculateDesignElevationPatchResponse not same after round trip serialisation");

      result.member.Heights.ForEach((x, y) => result.member.Heights.Cells[x, y].Should().Be(0f));
    }

    [Fact]
    public void Test_CalculateDesignElevationPatchArgument_NonNullHeights()
    {
      var argument = new CalculateDesignElevationPatchResponse
      {
        CalcResult = DesignProfilerRequestResult.OK,
        Heights = new ClientHeightLeafSubGrid()
      };

      argument.Heights.ForEach((x, y) => argument.Heights.Cells[x, y] = x + y);

      var result = SimpleBinarizableInstanceTester.TestClass(argument, "Custom CalculateDesignElevationPatchResponse not same after round trip serialisation");

      result.member.Heights.ForEach((x, y) => result.member.Heights.Cells[x, y].Should().Be(x + y));
    }
  }
}
