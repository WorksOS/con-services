using System;
using FluentAssertions;
using VSS.TRex.Designs.GridFabric.Events;
using VSS.TRex.Tests.BinarizableSerialization;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace VSS.TRex.Tests.Designs.GridFabric
{

  public class DesignChangedEventTests
  {
    [Fact]
    public void Creation()
    {
      var evt = new DesignChangedEvent();
      evt.Should().NotBeNull();
      evt.DesignRemoved.Should().BeFalse();
    }

    [Fact]
    public void FromToBinary()
    {
      Guid newGuid = Guid.NewGuid();

      var evt = new DesignChangedEvent
      {
        SiteModelUid = newGuid,
        DesignUid = newGuid,
        DesignRemoved = true,
        FileType =ImportedFileType.DesignSurface
      };

      TestBinarizable_ReaderWriterHelper.RoundTripSerialise(evt);
    }
  }

}
