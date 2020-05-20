using System;
using FluentAssertions;
using VSS.TRex.SiteModels.GridFabric.Events;
using VSS.TRex.Tests.BinarizableSerialization;
using Xunit;

namespace VSS.TRex.Tests.SiteModels.GridFabric
{
  public class SiteModelAttributesChangedEventTests
  {
    [Fact]
    public void Creation()
    {
      var evt = new SiteModelAttributesChangedEvent();
      evt.Should().NotBeNull();
      evt.AlignmentsModified.Should().BeFalse();
      evt.CsibModified.Should().BeFalse();
      evt.DesignsModified.Should().BeFalse();
      evt.ExistenceMapChangeMask.Should().BeNull();
      evt.ExistenceMapModified.Should().BeFalse();
      evt.MachineDesignsModified.Should().BeFalse();
      evt.MachineTargetValuesModified.Should().BeFalse();
      evt.MachinesModified.Should().BeFalse();
      evt.ProofingRunsModified.Should().BeFalse();
      evt.SurveyedSurfacesModified.Should().BeFalse();
      evt.SiteModelMarkedForDeletion.Should().BeFalse();
      evt.SiteModelID.Should().Be(Guid.Empty);
    }

    [Fact]
    public void FromToBinary()
    {
      Guid newGuid = Guid.NewGuid();

      var evt = new SiteModelAttributesChangedEvent
      {
        AlignmentsModified = true,
        CsibModified = true,
        DesignsModified = true,
        ExistenceMapChangeMask = new byte[0],
        ExistenceMapModified = true,
        MachineDesignsModified = true,
        MachineTargetValuesModified = true,
        MachinesModified = true,
        ProofingRunsModified = true,
        SurveyedSurfacesModified = true,
        SiteModelMarkedForDeletion = true,
        SiteModelID = newGuid
      };

      TestBinarizable_ReaderWriterHelper.RoundTripSerialise(evt);
    }
  }
}
