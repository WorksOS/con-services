using System;
using FluentAssertions;
using VSS.TRex.Designs.GridFabric.Arguments;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Designs
{
  public class ToFromBinary_AlignmentDesignGeometryArgument
  {
    [Fact]
    public void Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<AlignmentDesignGeometryArgument>("Empty AlignmentDesignGeometryArgument not same after round trip serialisation");
    }

    [Fact]
    public void WithData()
    {
      var projectUid = Guid.NewGuid();
      var designUid = Guid.NewGuid();

      var arg = new AlignmentDesignGeometryArgument { ProjectID = projectUid, AlignmentDesignID = designUid };

      var result = SimpleBinarizableInstanceTester.TestClass(arg, "Custom AlignmentDesignGeometryArgument not same after round trip serialisation");

      result.member.Should().BeEquivalentTo(arg);
    }
  }
}
