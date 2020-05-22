using System;
using System.Collections.Generic;
using System.Text;
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
    }

    [Fact]
    public void Serialization()
    {
      var projectUid = Guid.NewGuid();
      var designUid = Guid.NewGuid();

      var arg = new AlignmentDesignGeometryArgument {ProjectID = projectUid, AlignmentDesignID = designUid};

      arg.Should().NotBeNull();
    }

  }
}
