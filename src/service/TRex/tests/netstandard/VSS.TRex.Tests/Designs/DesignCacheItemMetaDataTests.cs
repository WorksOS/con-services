using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Nito.AsyncEx.Synchronous;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using Xunit;

namespace VSS.TRex.Tests.Designs
{
  public class DesignCacheItemMetaDataTests
  {
    [Fact]
    public void Creation()
    {
      var mockDesign = new Mock<IDesignBase>();
      var metadata = new DesignCacheItemMetaData(mockDesign.Object, 12345);

      metadata.Design.Should().Be(mockDesign.Object);
      metadata.SizeInCache.Should().Be(12345);
      metadata.LastTouchedDate.Should().BeAfter(DateTime.UtcNow.AddSeconds(-1));
    }

    [Fact]
    public void Touch()
    {
      var mockDesign = new Mock<IDesignBase>();
      var metadata = new DesignCacheItemMetaData(mockDesign.Object, 12345);

      metadata.Design.Should().Be(mockDesign.Object);
      metadata.SizeInCache.Should().Be(12345);
      metadata.LastTouchedDate.Should().BeAfter(DateTime.UtcNow.AddSeconds(-1));

      var referenceTouch = metadata.LastTouchedDate;
      Task.Delay(1).WaitAndUnwrapException();

      metadata.Touch();
      metadata.LastTouchedDate.Should().BeAfter(referenceTouch);
    }
  }
}
