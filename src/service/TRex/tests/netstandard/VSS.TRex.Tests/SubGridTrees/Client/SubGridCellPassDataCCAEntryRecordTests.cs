using System.IO;
using FluentAssertions;
using VSS.TRex.Common;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.SubGridTrees.Client.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  public class SubGridCellPassDataCCAEntryRecordTests
  {
    [Fact]
    public void Creation()
    {
      var rec = new SubGridCellPassDataCCAEntryRecord();

      rec.Clear();

      Assert.True(rec.MeasuredCCA == CellPassConsts.NullCCA);
      Assert.True(rec.TargetCCA == CellPassConsts.NullCCATarget);
      Assert.True(rec.PreviousMeasuredCCA == CellPassConsts.NullCCA);
      Assert.True(rec.PreviousTargetCCA == CellPassConsts.NullCCATarget);
    }

    [Fact]
    public void Creation2()
    {
      var rec = new SubGridCellPassDataCCAEntryRecord(1, 2, 3, 4);

      rec.MeasuredCCA.Should().Be(1);
      rec.TargetCCA.Should().Be(2);
      rec.PreviousMeasuredCCA.Should().Be(3);
      rec.PreviousTargetCCA.Should().Be(4);
    }

    [Fact]
    public void Clear()
    {
      var rec = new SubGridCellPassDataCCAEntryRecord(1, 2, 3, 4);
      rec.Clear();

      rec.Should().BeEquivalentTo(SubGridCellPassDataCCAEntryRecord.NullValue);
    }

    [Fact]
    public void NullValue()
    {
      var rec = SubGridCellPassDataCCAEntryRecord.NullValue;

      rec.MeasuredCCA.Should().Be(CellPassConsts.NullCCA);
      rec.TargetCCA.Should().Be(CellPassConsts.NullCCATarget);
      rec.PreviousMeasuredCCA.Should().Be(CellPassConsts.NullCCA);
      rec.PreviousTargetCCA.Should().Be(CellPassConsts.NullCCATarget);
      rec.IsDecoupled.Should().BeFalse();
      rec.IsOvercompacted.Should().BeFalse();
      rec.IsTooThick.Should().BeFalse();
      rec.IsTopLayerTooThick.Should().BeFalse();
      rec.IsTopLayerUndercompacted.Should().BeFalse();
      rec.IsUndercompacted.Should().BeFalse();
    }

    [Fact]
    public void Flags()
    {
      var rec = SubGridCellPassDataCCAEntryRecord.NullValue;

      rec.IsDecoupled.Should().BeFalse();
      rec.IsDecoupled = true;
      rec.IsDecoupled.Should().BeTrue();

      rec.IsOvercompacted.Should().BeFalse();
      rec.IsOvercompacted = true;
      rec.IsOvercompacted.Should().BeTrue();

      rec.IsTooThick.Should().BeFalse();
      rec.IsTooThick = true;
      rec.IsTooThick.Should().BeTrue();

      rec.IsTopLayerTooThick.Should().BeFalse();
      rec.IsTopLayerTooThick = true;
      rec.IsTopLayerTooThick.Should().BeTrue();

      rec.IsTopLayerUndercompacted.Should().BeFalse();
      rec.IsTopLayerUndercompacted = true;
      rec.IsTopLayerUndercompacted.Should().BeTrue();

      rec.IsUndercompacted.Should().BeFalse();
      rec.IsUndercompacted = true;
      rec.IsUndercompacted.Should().BeTrue();
    }

    [Fact]
    public void BinaryReaderWriter()
    {
      var instance = new SubGridCellPassDataCCAEntryRecord(1, 2, 3, 4);

      // Test using standard Read()/Write()
      var writer = new BinaryWriter(new MemoryStream(Consts.TREX_DEFAULT_MEMORY_STREAM_CAPACITY_ON_CREATION));
      instance.Write(writer);

      (writer.BaseStream as MemoryStream).Position = 0;
      var instance2 = new SubGridCellPassDataCCAEntryRecord();
      instance2.Read(new BinaryReader(writer.BaseStream as MemoryStream));

      instance.Should().BeEquivalentTo(instance2);
    }

    [Fact]
    public void IndicativeSizeInBytes()
    {
      SubGridCellPassDataCCAEntryRecord.IndicativeSizeInBytes().Should().Be(4 * sizeof(byte) + sizeof(byte));
    }
  }
}
