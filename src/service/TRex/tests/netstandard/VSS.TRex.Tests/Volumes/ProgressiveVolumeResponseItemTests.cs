using System;
using System.IO;
using FluentAssertions;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Tests.BinarizableSerialization;
using VSS.TRex.Volumes.GridFabric.Responses;
using Xunit;

namespace VSS.TRex.Tests.Volumes
{
  public class ProgressiveVolumeResponseItemTests
  {
    [Fact]
    public void Creation()
    {
      var item = new ProgressiveVolumeResponseItem();
      item.Should().NotBeNull();
    }

    [Fact]
    public void Serialization_Null()
    {
      var arg = new ProgressiveVolumeResponseItem();

      var writer = new TestBinaryWriter();
      arg.ToBinary(writer);

      var cp2 = new ProgressiveVolumeResponseItem();
      cp2.FromBinary(new TestBinaryReader(writer._stream.BaseStream as MemoryStream));

      arg.Should().BeEquivalentTo(cp2);
    }

    [Fact]
    public void Serialization_Complex()
    {
      var date = DateTime.UtcNow;

      var arg = new ProgressiveVolumeResponseItem
      {
        Date = date,
        Volume = new SimpleVolumesResponse
        {
          Cut = 10.0,
          Fill = 20.0,
          BoundingExtentGrid = new TRex.Geometry.BoundingWorldExtent3D(1.0, 2.0, 3.0, 4.0, 5.0, 6.0),
          CutArea = 30.0,
          FillArea = 40.0,
          TotalCoverageArea = 100.0
        }
      };

      var writer = new TestBinaryWriter();
      arg.ToBinary(writer);

      var cp2 = new ProgressiveVolumeResponseItem();
      cp2.FromBinary(new TestBinaryReader(writer._stream.BaseStream as MemoryStream));

      arg.Should().BeEquivalentTo(cp2);
    }
  }
}
