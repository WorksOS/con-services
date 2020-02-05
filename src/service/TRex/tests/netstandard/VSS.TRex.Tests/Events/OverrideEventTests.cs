using System;
using FluentAssertions;
using VSS.TRex.Events.Models;
using VSS.TRex.Types.CellPasses;
using Xunit;

namespace VSS.TRex.Tests.Events
{
  public class OverrideEventTests
  {
    [Fact]
    public void OverrideEvent_Creation()
    {
      var ed = DateTime.UtcNow;

      var evt = new OverrideEvent<bool>(ed, true);
      evt.EndDate.Should().Be(ed);
      evt.Value.Should().BeTrue();
    }

    [Fact]
    public void OverrideEvent_Null()
    {
      var evt = OverrideEvent<bool>.Null(false);
      evt.EndDate.Should().Be(CellPassConsts.NullTime);
      evt.Value.Should().BeFalse();
    }

  }
}
