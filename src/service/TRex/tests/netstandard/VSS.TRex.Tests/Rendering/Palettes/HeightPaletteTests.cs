using System.Drawing;
using FluentAssertions;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Rendering.Palettes;
using Xunit;

namespace VSS.TRex.Tests.Rendering.Palettes
{
  public class HeightPaletteTests
  {
    private const double MIN_ELEVATION = 100.0;
    private const double MAX_ELEVATION = 500.0;

    [Fact]
    public void Test_HeightPalette_Creation()
    {
      var palette = new HeightPalette();

      palette.Should().NotBeNull();

      palette.PaletteTransitions.Should().BeNull();

      palette.ElevationPalette.Should().NotBeNull();
      palette.ElevationPalette.Length.Should().Be(20);

      palette.ElevationPalette[0].Should().Be(Color.Aqua);
      palette.ElevationPalette[1].Should().Be(Color.Yellow);
      palette.ElevationPalette[2].Should().Be(Color.Fuchsia);
      palette.ElevationPalette[3].Should().Be(Color.Lime);
      palette.ElevationPalette[4].Should().Be(Color.FromArgb(0x80, 0x80, 0xFF));
      palette.ElevationPalette[5].Should().Be(Color.LightGray);
      palette.ElevationPalette[6].Should().Be(Color.FromArgb(0xEB, 0xFD, 0xAC));
      palette.ElevationPalette[7].Should().Be(Color.FromArgb(0xFF, 0x80, 0x00));
      palette.ElevationPalette[8].Should().Be(Color.FromArgb(0xFF, 0xC0, 0xFF));
      palette.ElevationPalette[9].Should().Be(Color.FromArgb(0x96, 0xCB, 0xFF));
      palette.ElevationPalette[10].Should().Be(Color.FromArgb(0xB5, 0x8E, 0x6C));
      palette.ElevationPalette[11].Should().Be(Color.FromArgb(0xFF, 0xFF, 0x80));
      palette.ElevationPalette[12].Should().Be(Color.FromArgb(0xFF, 0x80, 0x80));
      palette.ElevationPalette[13].Should().Be(Color.FromArgb(0x80, 0xFF, 0x00));
      palette.ElevationPalette[14].Should().Be(Color.FromArgb(0x00, 0x80, 0xFF));
      palette.ElevationPalette[15].Should().Be(Color.FromArgb(0xFF, 0x00, 0x80));
      palette.ElevationPalette[16].Should().Be(Color.Teal);
      palette.ElevationPalette[17].Should().Be(Color.FromArgb(0xFF, 0xC0, 0xC0));
      palette.ElevationPalette[18].Should().Be(Color.FromArgb(0xFF, 0x80, 0xFF));
      palette.ElevationPalette[19].Should().Be(Color.FromArgb(0x00, 0xFF, 0x80));
    }

    [Fact]
    public void Test_HeightPalette_ChooseColour_NullElevation()
    {
      var palette = new HeightPalette(MIN_ELEVATION, MAX_ELEVATION);

      palette.Should().NotBeNull();

      for (var i = 0; i < palette.ElevationPalette.Length; i++)
        palette.ChooseColour(CellPassConsts.NullHeight).Should().Be(Color.Black);
    }

    [Fact]
    public void Test_HeightPalette_ChooseColour()
    {
      const double START_ELEVATION = 110.0;
      const double ELEVATION_INCREMENT = 20.0;
      
      var palette = new HeightPalette(MIN_ELEVATION, MAX_ELEVATION);

      palette.Should().NotBeNull();

      for (var i = 0; i < palette.ElevationPalette.Length; i++)
        palette.ChooseColour(START_ELEVATION + i * ELEVATION_INCREMENT).Should().Be(palette.ElevationPalette[i]);
    }
  }
}
