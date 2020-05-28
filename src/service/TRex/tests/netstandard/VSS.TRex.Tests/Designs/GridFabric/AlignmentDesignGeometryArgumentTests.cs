using System;
using FluentAssertions;
using VSS.TRex.Designs.GridFabric.Arguments;
using Xunit;

namespace VSS.TRex.Tests.Designs.GridFabric
{
  public class AlignmentDesignGeometryArgumentTests
  {
    [Fact]
    public void Creation()
    {
      var arg = new AlignmentDesignGeometryArgument();
      arg.Should().NotBeNull();
      arg.ProjectID.Should().Be(Guid.Empty);
      arg.AlignmentDesignID.Should().Be(Guid.Empty);
      arg.ConvertArcsToPolyLines.Should().BeFalse();
      arg.ArcChordTolerance.Should().Be(1.0d);
    }
  }
}
