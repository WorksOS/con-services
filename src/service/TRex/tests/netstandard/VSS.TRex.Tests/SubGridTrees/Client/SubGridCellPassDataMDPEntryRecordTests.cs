using System.IO;
using FluentAssertions;
using VSS.TRex.Common;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.SubGridTrees.Client.Types;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  public class SubGridCellPassDataMDPEntryRecordTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_SubGridCellPassDataMDPEntryRecord_Creation()
    {
      SubGridCellPassDataMDPEntryRecord rec = new SubGridCellPassDataMDPEntryRecord();

      rec.Clear();

      Assert.True(rec.MeasuredMDP == CellPassConsts.NullMDP);
      Assert.True(rec.TargetMDP == CellPassConsts.NullMDP);
    }

    [Fact]
    public void Creation2()
    {
      var rec = new SubGridCellPassDataMDPEntryRecord(1, 2);

      rec.MeasuredMDP.Should().Be(1);
      rec.TargetMDP.Should().Be(2);
    }

    [Fact]
    public void Clear()
    {
      var rec = new SubGridCellPassDataMDPEntryRecord(1, 2);
      rec.Clear();

      rec.Should().BeEquivalentTo(SubGridCellPassDataMDPEntryRecord.NullValue);
    }

    [Fact]
    public void NullValue()
    {
      var rec = SubGridCellPassDataMDPEntryRecord.NullValue;

      rec.MeasuredMDP.Should().Be(CellPassConsts.NullCCV);
      rec.TargetMDP.Should().Be(CellPassConsts.NullCCV);
      rec.IsOvercompacted.Should().BeFalse();
      rec.IsTooThick.Should().BeFalse();
      rec.IsTopLayerTooThick.Should().BeFalse();
      rec.IsTopLayerUndercompacted.Should().BeFalse();
      rec.IsUndercompacted.Should().BeFalse();
    }

    [Fact]
    public void Flags()
    {
      var rec = SubGridCellPassDataMDPEntryRecord.NullValue;

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
      var instance = new SubGridCellPassDataMDPEntryRecord(1, 2);

      // Test using standard Read()/Write()
      var writer = new BinaryWriter(new MemoryStream(Consts.TREX_DEFAULT_MEMORY_STREAM_CAPACITY_ON_CREATION));
      instance.Write(writer);

      (writer.BaseStream as MemoryStream).Position = 0;
      var instance2 = new SubGridCellPassDataMDPEntryRecord();
      instance2.Read(new BinaryReader(writer.BaseStream as MemoryStream));

      instance.Should().BeEquivalentTo(instance2);
    }

    [Fact]
    public void IndicativeSizeInBytes()
    {
      SubGridCellPassDataMDPEntryRecord.IndicativeSizeInBytes().Should().Be(2 * sizeof(short) + sizeof(byte));
    }
  }
}
