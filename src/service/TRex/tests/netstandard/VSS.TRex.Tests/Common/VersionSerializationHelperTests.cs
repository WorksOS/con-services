using System;
using System.IO;
using FluentAssertions;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Tests.BinarizableSerialization;
using Xunit;

namespace VSS.TRex.Tests.Common
{
  public class VersionSerializationHelperTests
  {
    [Fact]
    public void EmitVersionByte_Binarizable()
    {
      const byte TEST_VERSION = 99;

      var writer = new TestBinaryWriter();
      VersionSerializationHelper.EmitVersionByte(writer, TEST_VERSION);

      var ms = writer._stream.BaseStream as MemoryStream;
      ms.Position = 0;

      ms.ReadByte().Should().Be(TEST_VERSION);
    }

    [Fact]
    public void CheckVersionByte_Binarizable_Failure()
    {
      const byte TEST_VERSION = 99;
      const byte BAD_TEST_VERSION = 100;

      var writer = new TestBinaryWriter();
      VersionSerializationHelper.EmitVersionByte(writer, BAD_TEST_VERSION);

      var ms = writer._stream.BaseStream as MemoryStream;
      ms.Position = 0;

      var reader = new TestBinaryReader(ms);
      
      Action act = () => VersionSerializationHelper.CheckVersionByte(reader, TEST_VERSION);
      act.Should().Throw<TRexSerializationVersionException>().WithMessage($"Invalid version read during deserialization: {BAD_TEST_VERSION}, expected version in [{TEST_VERSION}]");
    }

    [Fact]
    public void CheckVersionByte_Binarizable_Success()
    {
      const byte TEST_VERSION = 99;

      var writer = new TestBinaryWriter();
      VersionSerializationHelper.EmitVersionByte(writer, TEST_VERSION);

      var ms = writer._stream.BaseStream as MemoryStream;
      ms.Position = 0;

      var reader = new TestBinaryReader(ms);
      VersionSerializationHelper.CheckVersionByte(reader, TEST_VERSION);
    }

    [Fact]
    public void CheckVersionsByte_Binarizable_Failure()
    {
      const byte TEST_VERSION = 99;
      const byte BAD_TEST_VERSION = 99;
      byte[] EXPECTED_VERSIONS = {97, 98};      

      var writer = new TestBinaryWriter();
      VersionSerializationHelper.EmitVersionByte(writer, BAD_TEST_VERSION);

      var ms = writer._stream.BaseStream as MemoryStream;
      ms.Position = 0;

      var reader = new TestBinaryReader(ms);

      Action act = () => VersionSerializationHelper.CheckVersionsByte(reader, EXPECTED_VERSIONS);
      act.Should().Throw<TRexSerializationVersionException>().WithMessage($"Invalid version read during deserialization: {TEST_VERSION}, expected version in [{string.Join(", ", EXPECTED_VERSIONS)}]");
    }

    [Fact]
    public void CheckVersionsByte_Binarizable_Success()
    {
      const byte TEST_VERSION = 99;
      byte[] EXPECTED_VERSIONS = { 97, 98, 99 };

      var writer = new TestBinaryWriter();
      VersionSerializationHelper.EmitVersionByte(writer, TEST_VERSION);

      var ms = writer._stream.BaseStream as MemoryStream;
      ms.Position = 0;

      var reader = new TestBinaryReader(ms);
      VersionSerializationHelper.CheckVersionsByte(reader, EXPECTED_VERSIONS);
    }

    [Fact]
    public void EmitVersionByte_BinaryReaderWriter()
    {
      const byte TEST_VERSION = 99;

      var writer = new BinaryWriter(new MemoryStream(Consts.TREX_DEFAULT_MEMORY_STREAM_CAPACITY_ON_CREATION));
      VersionSerializationHelper.EmitVersionByte(writer, TEST_VERSION);

      var ms = writer.BaseStream as MemoryStream;
      ms.Position = 0;

      ms.ReadByte().Should().Be(TEST_VERSION);
    }

    [Fact]
    public void CheckVersionByte_BinaryReaderWriter_Failure()
    {
      const byte TEST_VERSION = 99;
      const byte BAD_TEST_VERSION = 100;

      var writer = new BinaryWriter(new MemoryStream(Consts.TREX_DEFAULT_MEMORY_STREAM_CAPACITY_ON_CREATION));
      VersionSerializationHelper.EmitVersionByte(writer, BAD_TEST_VERSION);

      var ms = writer.BaseStream as MemoryStream;
      ms.Position = 0;

      var reader = new BinaryReader(ms);

      Action act = () => VersionSerializationHelper.CheckVersionByte(reader, TEST_VERSION);
      act.Should().Throw<TRexSerializationVersionException>().WithMessage($"Invalid version read during deserialization: {BAD_TEST_VERSION}, expected version in [{TEST_VERSION}]");
    }

    [Fact]
    public void CheckVersionByte_BinaryReaderWriter_Success()
    {
      const byte TEST_VERSION = 99;

      var writer = new BinaryWriter(new MemoryStream(Consts.TREX_DEFAULT_MEMORY_STREAM_CAPACITY_ON_CREATION));
      VersionSerializationHelper.EmitVersionByte(writer, TEST_VERSION);

      var ms = writer.BaseStream as MemoryStream;
      ms.Position = 0;

      var reader = new BinaryReader(ms);
      VersionSerializationHelper.CheckVersionByte(reader, TEST_VERSION);
    }

    [Fact]
    public void CheckVersionsByte_BinaryReaderWriter_Failure()
    {
      const byte TEST_VERSION = 99;
      const byte BAD_TEST_VERSION = 99;
      byte[] EXPECTED_VERSIONS = { 97, 98 };

      var writer = new BinaryWriter(new MemoryStream(Consts.TREX_DEFAULT_MEMORY_STREAM_CAPACITY_ON_CREATION));
      VersionSerializationHelper.EmitVersionByte(writer, BAD_TEST_VERSION);

      var ms = writer.BaseStream as MemoryStream;
      ms.Position = 0;

      var reader = new BinaryReader(ms);

      Action act = () => VersionSerializationHelper.CheckVersionsByte(reader, EXPECTED_VERSIONS);
      act.Should().Throw<TRexSerializationVersionException>().WithMessage($"Invalid version read during deserialization: {TEST_VERSION}, expected version in [{string.Join(", ", EXPECTED_VERSIONS)}]");
    }

    [Fact]
    public void CheckVersionsByte__BinaryReaderWriter_Success()
    {
      const byte TEST_VERSION = 99;
      byte[] EXPECTED_VERSIONS = { 97, 98, 99 };

      var writer = new BinaryWriter(new MemoryStream(Consts.TREX_DEFAULT_MEMORY_STREAM_CAPACITY_ON_CREATION));
      VersionSerializationHelper.EmitVersionByte(writer, TEST_VERSION);

      var ms = writer.BaseStream as MemoryStream;
      ms.Position = 0;

      var reader = new BinaryReader(ms);
      VersionSerializationHelper.CheckVersionsByte(reader, EXPECTED_VERSIONS);
    }
  }
}
