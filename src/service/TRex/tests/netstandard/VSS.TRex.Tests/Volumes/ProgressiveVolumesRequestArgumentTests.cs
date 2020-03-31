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
  public class ProgressiveVolumesRequestArgumentTests : IClassFixture<DITAGFileAndSubGridRequestsFixture>
  {
    [Fact]
    public void Creation()
    {
      var arg = new ProgressiveVolumesRequestArgument();

      Assert.NotNull(arg);
    }

    [Fact]
    public void Serialization_Null()
    {
      var arg = new ProgressiveVolumesRequestArgument();

      var writer = new TestBinaryWriter();
      arg.ToBinary(writer);

      var cp2 = new ProgressiveVolumesRequestArgument();
      cp2.FromBinary(new TestBinaryReader(writer._stream.BaseStream as MemoryStream));

      arg.Should().BeEquivalentTo(cp2);
    }

    [Fact]
    public void Serialization_Complex()
    {
      var arg = new ProgressiveVolumesRequestArgument
      {
        Interval = new TimeSpan(1, 0, 0, 0),
        StartDate = new DateTime(2020, 1, 1, 1, 1, 1),
        EndDate = new DateTime(2020, 1, 1, 1, 1, 1),
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

      var cp2 = new ProgressiveVolumesRequestArgument();
      cp2.FromBinary(new TestBinaryReader(writer._stream.BaseStream as MemoryStream));

      arg.Should().BeEquivalentTo(cp2);
    }
  }
}
