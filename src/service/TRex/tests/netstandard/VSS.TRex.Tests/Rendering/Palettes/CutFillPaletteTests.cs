using System.Drawing;
using FluentAssertions;
using VSS.TRex.Rendering.Palettes;
using Xunit;

namespace VSS.TRex.Tests.Rendering.Palettes
{
  public class CutFillPaletteTests
  {
    private const double CUT_FILL_VALUE = -2.5;
    private const double VALUE_INCREMENT = 1.0;

    [Fact]
    public void Test_CutFillPalette_Creation()
    {
      var palette = new CutFillPalette();

      palette.Should().NotBeNull();

      palette.PaletteTransitions.Should().NotBeNull();
      palette.PaletteTransitions.Length.Should().Be(8);
      palette.PaletteTransitions[0].Value.Should().Be(-10000.0);
      palette.PaletteTransitions[0].Color.Should().Be(Color.Red);
      palette.PaletteTransitions[1].Value.Should().Be(-2.0);
      palette.PaletteTransitions[1].Color.Should().Be(Color.Red);
      palette.PaletteTransitions[2].Value.Should().Be(-1.0);
      palette.PaletteTransitions[2].Color.Should().Be(Color.Yellow);
      palette.PaletteTransitions[3].Value.Should().Be(-0.1);
      palette.PaletteTransitions[3].Color.Should().Be(Color.Green);
      palette.PaletteTransitions[4].Value.Should().Be(0.1);
      palette.PaletteTransitions[4].Color.Should().Be(Color.Blue);
      palette.PaletteTransitions[5].Value.Should().Be(1.0);
      palette.PaletteTransitions[5].Color.Should().Be(Color.SkyBlue);
      palette.PaletteTransitions[6].Value.Should().Be(2.0);
      palette.PaletteTransitions[6].Color.Should().Be(Color.DarkBlue);
      palette.PaletteTransitions[7].Value.Should().Be(10000.0);
      palette.PaletteTransitions[7].Color.Should().Be(Color.DarkBlue);
    }

    [Fact]
    public void Test_CutFillPalette_ChooseColour_NoTargetCCVColourInPVM()
    {
      var palette = new CutFillPalette();

      palette.Should().NotBeNull();
      
      var colors = new[] { Color.Red, Color.Red, Color.Yellow, Color.Blue, Color.SkyBlue, Color.DarkBlue, Color.DarkBlue };

      for (var i = 0; i < colors.Length; i++)
        palette.ChooseColour(CUT_FILL_VALUE + i * VALUE_INCREMENT).Should().Be(colors[i]);

      palette.ChooseColour(0).Should().Be(Color.Green);
    }

  }
}
