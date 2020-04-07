using System;
using System.IO;
using FluentAssertions;
using VSS.TRex.Common.Models;
using VSS.TRex.Filters;
using VSS.TRex.Tests.BinarizableSerialization;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Volumes.GridFabric.Arguments;
using Xunit;

namespace VSS.TRex.Tests.Volumes
{
  public class ProgressiveVolumesSubGridsRequestArgumentTests : IClassFixture<DITAGFileAndSubGridRequestsFixture>
  {
    [Fact]
    public void Creation()
    {
      var arg = new ProgressiveVolumesSubGridsRequestArgument();

      Assert.NotNull(arg);
    }

    [Fact]
    public void Serialization_Null()
    {
      var arg = new ProgressiveVolumesSubGridsRequestArgument();

      var writer = new TestBinaryWriter();
      arg.ToBinary(writer);

      var cp2 = new ProgressiveVolumesSubGridsRequestArgument();
      cp2.FromBinary(new TestBinaryReader(writer._stream.BaseStream as MemoryStream));

      arg.Should().BeEquivalentTo(cp2);
    }

    [Fact]
    public void Serialization_Complex()
    {
      var arg = new ProgressiveVolumesSubGridsRequestArgument
      {
        Interval = new TimeSpan(1, 0, 0, 0),
        StartDate = new DateTime(2020, 1, 1, 1, 1, 1),
        EndDate = new DateTime(2020, 1, 1, 1, 1, 1),
        ExternalDescriptor = Guid.NewGuid(),
        Filters = new FilterSet(new CombinedFilter()),
        LiftParams = new LiftParameters(),
        OriginatingIgniteNodeId = Guid.NewGuid(),
        Overrides = new OverrideParameters(),
        ProjectID = Guid.NewGuid()
      };

      var writer = new TestBinaryWriter();
      arg.ToBinary(writer);

      var cp2 = new ProgressiveVolumesSubGridsRequestArgument();
      cp2.FromBinary(new TestBinaryReader(writer._stream.BaseStream as MemoryStream));

      arg.Should().BeEquivalentTo(cp2);
    }
  }
}
