using FluentAssertions;
using VSS.TRex.Filters.Models;
using Xunit;

namespace VSS.TRex.Tests.Filters
{
  public class FilteredValueAssignmentContextTests
  {
    [Fact]
    public void Creation()
    {
      var fva = new FilteredValueAssignmentContext();
      fva.Should().NotBeNull();
      fva.CellProfile.Should().BeNull();
      fva.ProbePositions.Should().NotBeNull();
      fva.Overrides.Should().NotBeNull();
    }

    [Fact]
    public void ProbePoint_Creation()
    {
      var pp = new FilteredValueAssignmentContext.ProbePoint(12.3f, 23.4f);
      pp.XOffset.Should().Be(12.3f);
      pp.YOffset.Should().Be(23.4f);
    }

    [Fact]
    public void ProbePoint_SetOffsets()
    {
      var fva = new FilteredValueAssignmentContext();
      fva.ProbePositions[10, 11].SetOffsets(12.3f, 23.4f);
      fva.ProbePositions[10, 11].XOffset.Should().Be(12.3f);
      fva.ProbePositions[10, 11].YOffset.Should().Be(23.4f);
    }
  }
}
