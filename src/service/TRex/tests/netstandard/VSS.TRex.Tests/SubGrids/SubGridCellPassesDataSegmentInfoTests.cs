using System;
using System.IO;
using System.Threading;
using FluentAssertions;

using VSS.TRex.SubGridTrees.Server;
using Xunit;

namespace VSS.TRex.Tests.SubGrids
{
  public class SubGridCellPassesDataSegmentInfoTests
  {
    [Fact]
    public void Creation()
    {
      var info = new SubGridCellPassesDataSegmentInfo();
      info.Should().NotBeNull();
    }

    [Fact]
    public void Creation2()
    {
      var startDate = DateTime.UtcNow;
      var endDate = startDate.AddMinutes(1);
      var segment = new SubGridCellPassesDataSegment();
      var info = new SubGridCellPassesDataSegmentInfo(startDate, endDate, segment);

      info.StartTime.Should().Be(startDate);
      info.EndTime.Should().Be(endDate);
      info.Segment.Should().BeEquivalentTo(segment);
    }

    [Fact]
    public void IncludesTimeWithinBounds()
    {
      var startDate = DateTime.UtcNow;
      var endDate = startDate.AddMinutes(1);
      var info = new SubGridCellPassesDataSegmentInfo(startDate, endDate, null);

      info.IncludesTimeWithinBounds(new DateTime(startDate.Ticks + 1)).Should().BeTrue();
      info.IncludesTimeWithinBounds(new DateTime(endDate.Ticks - 1)).Should().BeTrue();
      info.IncludesTimeWithinBounds(new DateTime((startDate.Ticks + endDate.Ticks) / 2)).Should().BeTrue();

      info.IncludesTimeWithinBounds(new DateTime(startDate.Ticks)).Should().BeFalse();
      info.IncludesTimeWithinBounds(new DateTime(endDate.Ticks)).Should().BeFalse();
    }

    [Fact]
    public void SegmentIdentifier()
    {
      var startDate = DateTime.UtcNow;
      var endDate = startDate.AddMinutes(1);
      var info = new SubGridCellPassesDataSegmentInfo(startDate, endDate, null);

      info.SegmentIdentifier().Should().Be(startDate.Ticks + "-" + endDate.Ticks);
    }

    [Fact]
    public void FileName()
    {
      var startDate = DateTime.UtcNow;
      var endDate = startDate.AddMinutes(1);
      var info = new SubGridCellPassesDataSegmentInfo(startDate, endDate, null);

      info.FileName(1234, 5678).Should().Be($"{info.Version}-{1234:d10}-{5678:d10}-{info.SegmentIdentifier()}");
    }

    [Fact]
    public void Serialization_NoMachineDirectory()
    {
      var startDate = DateTime.UtcNow;
      var endDate = startDate.AddMinutes(1);
      var info = new SubGridCellPassesDataSegmentInfo(startDate, endDate, null) {MachineDirectory = null, MaxElevation = 1234.56, MinElevation = 123.45};

      using var ms = new MemoryStream();
      using var bw = new BinaryWriter(ms);

      info.Write(bw);

      ms.Position = 0;
      using var br = new BinaryReader(ms);
      var newInfo = new SubGridCellPassesDataSegmentInfo();
      newInfo.Read(br);

      newInfo.Should().BeEquivalentTo(info);
    }

    [Fact]
    public void Serialization_WithMachineDirectory()
    {
      var startDate = DateTime.UtcNow;
      var endDate = startDate.AddMinutes(1);
      var info = new SubGridCellPassesDataSegmentInfo(startDate, endDate, null)
      {
        MachineDirectory = new short[]{1, 2, 3, 4, 5},
        MaxElevation = 1234.56,
        MinElevation = 123.45
      };

      using var ms = new MemoryStream();
      using var bw = new BinaryWriter(ms);

      info.Write(bw);

      ms.Position = 0;
      using var br = new BinaryReader(ms);
      var newInfo = new SubGridCellPassesDataSegmentInfo();
      newInfo.Read(br);

      newInfo.Should().BeEquivalentTo(info);
    }

    [Fact]
    public void Touch()
    {
      var startDate = DateTime.UtcNow;
      var endDate = startDate.AddMinutes(1);
      var info = new SubGridCellPassesDataSegmentInfo(startDate, endDate, null);

      // set initial touch version
      info.Touch();

      var currentTime = DateTime.UtcNow;
      Thread.Sleep(100);

      info.Touch();
      var timeFromVersion = new DateTime(info.Version);
      timeFromVersion.Should().BeAfter(currentTime);

      currentTime.AddMilliseconds(200).Should().BeAfter(timeFromVersion);
    }
  }
}
