using System;
using System.IO;
using FluentAssertions;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.TAGFiles.Models;
using VSS.TRex.Tests.BinarizableSerialization;
using Xunit;

namespace TAGFiles.Tests
{
  public class SegmentRetirementQueueItemTests
  {
    [Fact]
    public void Test_SegmentRetirementQueueItem_Creation()
    {
      var item = new SegmentRetirementQueueItem();

      item.Should().NotBeNull();
    }

    [Fact]
    public void Test_SegmentRetirementQueueItem_SerializationVersionFail()
    {
      var expectedVersions = new int[] {SegmentRetirementQueueItem.VERSION_NUMBER};
      var writer = new TestBinaryWriter();
      writer.WriteByte(SegmentRetirementQueueItem.VERSION_NUMBER + 1);
      var reader = new TestBinaryReader(writer._stream.BaseStream as MemoryStream);

      var item = new SegmentRetirementQueueItem();
      Action act = () => item.ReadBinary(reader);

      act.Should().Throw<TRexSerializationVersionException>().WithMessage(TRexSerializationVersionException.ErrorMessage(expectedVersions, SegmentRetirementQueueItem.VERSION_NUMBER + 1));
    }
  }
}
