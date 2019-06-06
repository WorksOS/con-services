using System;
using System.IO;
using FluentAssertions;
using Moq;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Serialisation;
using VSS.TRex.Tests.BinarizableSerialization;
using Xunit;

namespace VSS.TRex.Tests.Common
{
  public class BinarizableSerializerTests
  {
    [Fact]
    public void Creation()
    {
      var bs = new BinarizableSerializer();
      bs.Should().NotBeNull();
    }

    [Fact]
    public void UnsupportedClass_Read()
    {
      var bs = new BinarizableSerializer();
      var reader = new TestBinaryReader(new MemoryStream(Consts.TREX_DEFAULT_MEMORY_STREAM_CAPACITY_ON_CREATION));
      Action act = () => bs.ReadBinary(new object(), reader);

      act.Should().Throw<TRexNonBinarizableException>().WithMessage("Not IBinarizable on ReadBinary: System.Object");
    }

    [Fact]
    public void Exception_Read()
    {
      var bs = new BinarizableSerializer();
      var writer = new Mock<TestBinaryWriter>();
      writer.Setup(x => x.WriteObject("Exception", It.IsAny<object>())).Callback((string fieldName, object val) => { });

      bs.WriteBinary(new TRexException("An Exception"), writer.Object);

      //act.Should().Throw<NotImplementedException>();

      var reader = new Mock<TestBinaryReader>(writer.Object._stream.BaseStream as MemoryStream);
      reader.Setup(x => x.ReadObject<Exception>("Exception")).Returns(new Exception("An exception"));

      var e = new Exception("");

      bs.ReadBinary(e, reader.Object);
      e.Message.Should().Be("An exception");
      //act.Should().Throw<NotImplementedException>();
    }

    [Fact]
    public void UnsupportedClass_Write()
    {
      var bs = new BinarizableSerializer();
      var writer = new TestBinaryWriter();
      Action act = () => bs.WriteBinary(new object(), writer);

      act.Should().Throw<TRexNonBinarizableException>().WithMessage("Not IBinarizable on WriteBinary: System.Object");
    }


    [Fact]
    public void Exception_Write()
    {
      var bs = new BinarizableSerializer();
      var writer = new TestBinaryWriter();
      Action act = () => bs.WriteBinary(new Exception("An Exception"), writer);

      act.Should().Throw<NotImplementedException>();
    }
  }
}
