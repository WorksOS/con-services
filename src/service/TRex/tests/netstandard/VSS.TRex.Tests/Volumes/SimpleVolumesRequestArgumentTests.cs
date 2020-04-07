using System;
using System.IO;
using FluentAssertions;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Tests.BinarizableSerialization;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Volumes.GridFabric.Arguments;
using Xunit;

namespace VSS.TRex.Tests.Volumes
{
  public class SimpleVolumesRequestArgumentTests : IClassFixture<DITAGFileAndSubGridRequestsFixture>
  {
    [Fact]
    public void Test_SimpleVolumesRequestArgument_Creation()
    {
      var arg = new SimpleVolumesRequestArgument();

      Assert.NotNull(arg);
    }

    [Fact]
    public void Serialization_Null()
    {
      var arg = new SimpleVolumesRequestArgument();

      var writer = new TestBinaryWriter();
      arg.ToBinary(writer);

      var cp2 = new SimpleVolumesRequestArgument();
      cp2.FromBinary(new TestBinaryReader(writer._stream.BaseStream as MemoryStream));

      arg.Should().BeEquivalentTo(cp2);
    }

    [Fact]
    public void Serialization_Complex()
    {
      var arg = new SimpleVolumesRequestArgument
      {
        CutTolerance = 0.05,
        FillTolerance = 0.01,
        AdditionalSpatialFilter = new CombinedFilter(),
        ExternalDescriptor = Guid.NewGuid(),
        Filters = new FilterSet(new CombinedFilter()),
        LiftParams = new LiftParameters(),
        OriginatingIgniteNodeId = Guid.NewGuid(),
        Overrides = new OverrideParameters(),
        ProjectID = Guid.NewGuid(),
        BaseDesign = new DesignOffset(Guid.NewGuid(), 1.23),
        TopDesign = new DesignOffset(Guid.NewGuid(), 1.33),
        VolumeType = VolumeComputationType.BetweenDesignAndFilter
      };

      var writer = new TestBinaryWriter();
      arg.ToBinary(writer);

      var cp2 = new SimpleVolumesRequestArgument();
      cp2.FromBinary(new TestBinaryReader(writer._stream.BaseStream as MemoryStream));

      arg.Should().BeEquivalentTo(cp2);
    }
  }
}
