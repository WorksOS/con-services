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
      palette.ElevationPalette.Length.Should().Be(31);

      palette.ElevationPalette[0].Should().Be(Color.FromArgb(200, 0, 0));
      palette.ElevationPalette[1].Should().Be(Color.FromArgb(255, 0, 0));
      palette.ElevationPalette[2].Should().Be(Color.FromArgb(225, 60, 0));
      palette.ElevationPalette[3].Should().Be(Color.FromArgb(255, 90, 0));
      palette.ElevationPalette[4].Should().Be(Color.FromArgb(255, 130, 0));
      palette.ElevationPalette[5].Should().Be(Color.FromArgb(255, 170, 0));
      palette.ElevationPalette[6].Should().Be(Color.FromArgb(255, 200, 0));
      palette.ElevationPalette[7].Should().Be(Color.FromArgb(255, 220, 0));
      palette.ElevationPalette[8].Should().Be(Color.FromArgb(250, 230, 0));
      palette.ElevationPalette[9].Should().Be(Color.FromArgb(220, 230, 0));
      palette.ElevationPalette[10].Should().Be(Color.FromArgb(210, 230, 0));
      palette.ElevationPalette[11].Should().Be(Color.FromArgb(200, 230, 0));
      palette.ElevationPalette[12].Should().Be(Color.FromArgb(180, 230, 0));
      palette.ElevationPalette[13].Should().Be(Color.FromArgb(150, 230, 0));
      palette.ElevationPalette[14].Should().Be(Color.FromArgb(130, 230, 0));
      palette.ElevationPalette[15].Should().Be(Color.FromArgb(100, 240, 0));
      palette.ElevationPalette[16].Should().Be(Color.FromArgb(0, 255, 0));
      palette.ElevationPalette[17].Should().Be(Color.FromArgb(0, 240, 100));
      palette.ElevationPalette[18].Should().Be(Color.FromArgb(0, 230, 130));
      palette.ElevationPalette[19].Should().Be(Color.FromArgb(0, 230, 150));
      palette.ElevationPalette[20].Should().Be(Color.FromArgb(0, 230, 180));
      palette.ElevationPalette[21].Should().Be(Color.FromArgb(0, 230, 200));
      palette.ElevationPalette[22].Should().Be(Color.FromArgb(0, 230, 210));
      palette.ElevationPalette[23].Should().Be(Color.FromArgb(0, 220, 220));
      palette.ElevationPalette[24].Should().Be(Color.FromArgb(0, 200, 230));
      palette.ElevationPalette[25].Should().Be(Color.FromArgb(0, 180, 240));
      palette.ElevationPalette[26].Should().Be(Color.FromArgb(0, 150, 245));
      palette.ElevationPalette[27].Should().Be(Color.FromArgb(0, 120, 250));
      palette.ElevationPalette[28].Should().Be(Color.FromArgb(0, 90, 255));
      palette.ElevationPalette[29].Should().Be(Color.FromArgb(0, 70, 255));
      palette.ElevationPalette[30].Should().Be(Color.FromArgb(0, 0, 255));
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
      const double ELEVATION_INCREMENT = 13.33333333333333;
      
      var palette = new HeightPalette(MIN_ELEVATION, MAX_ELEVATION);

      palette.Should().NotBeNull();

      for (var i = 0; i < palette.ElevationPalette.Length; i++)
        palette.ChooseColour(START_ELEVATION + i * ELEVATION_INCREMENT).Should().Be(palette.ElevationPalette[i]);
    }
  }
}
