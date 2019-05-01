using System.Drawing;
using FluentAssertions;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client.Types;
using Xunit;

namespace VSS.TRex.Tests.Rendering.Palettes
{
  public class CCASummaryPaletteTests
  {
    [Fact]
    public void Test_SpeedSummaryPalette_Creation()
    {
      var palette = new CCASummaryPalette();

      palette.Should().NotBeNull();

      palette.OvercompactedColour.Should().Be(Color.Yellow);
      palette.CompactedColour.Should().Be(Color.Red);
      palette.UndercompactedColour.Should().Be(Color.Aqua);

      palette.PaletteTransitions.Should().BeNull();
    }

    [Fact]
    public void Test_CCASummaryPalette_ChooseColour_Overcompacted()
    {
      var palette = new CCASummaryPalette();

      palette.Should().NotBeNull();

      var ccaCellData = new SubGridCellPassDataCCAEntryRecord
      {
        IsOvercompacted = true
      };

      palette.ChooseColour(ccaCellData).Should().Be(palette.OvercompactedColour);
    }

    [Fact]
    public void Test_CCASummaryPalette_ChooseColour_Compacted()
    {
      var palette = new CCASummaryPalette();

      palette.Should().NotBeNull();

      var ccaCellData = new SubGridCellPassDataCCAEntryRecord();

      palette.ChooseColour(ccaCellData).Should().Be(palette.CompactedColour);
    }

    [Fact]
    public void Test_CCASummaryPalette_ChooseColour_Undercompacted()
    {
      var palette = new CCASummaryPalette();

      palette.Should().NotBeNull();

      var ccaCellData = new SubGridCellPassDataCCAEntryRecord
      {
        IsUndercompacted = true
      };

      palette.ChooseColour(ccaCellData).Should().Be(palette.UndercompactedColour);
    }
  }
}
