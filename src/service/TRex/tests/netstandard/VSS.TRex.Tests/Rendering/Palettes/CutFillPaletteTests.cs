using System.Drawing;
using FluentAssertions;
using VSS.TRex.Rendering.Palettes;
using Xunit;

namespace VSS.TRex.Tests.Rendering.Palettes
{
  public class CutFillPaletteTests
  {
    private const double CUT_FILL_VALUE = -0.25;
    private const double VALUE_INCREMENT = 0.1;

    [Fact]
    public void Test_CutFillPalette_Creation()
    {
      var palette = new CutFillPalette();

      palette.Should().NotBeNull();

      palette.PaletteTransitions[0].Value.Should().Be(0.2);
      palette.PaletteTransitions[0].Color.Should().Be(Color.DarkBlue);

      palette.PaletteTransitions[1].Value.Should().Be(0.1);
      palette.PaletteTransitions[1].Color.Should().Be(Color.SkyBlue);

      palette.PaletteTransitions[2].Value.Should().Be(0.05);
      palette.PaletteTransitions[2].Color.Should().Be(Color.Blue);

      palette.PaletteTransitions[3].Value.Should().Be(0.0);
      palette.PaletteTransitions[3].Color.Should().Be(Color.Green);

      palette.PaletteTransitions[4].Value.Should().Be(-0.05);
      palette.PaletteTransitions[4].Color.Should().Be(Color.Yellow);

      palette.PaletteTransitions[5].Value.Should().Be(-0.1);
      palette.PaletteTransitions[5].Color.Should().Be(Color.Crimson);

      palette.PaletteTransitions[6].Value.Should().Be(-0.2);
      palette.PaletteTransitions[6].Color.Should().Be(Color.Red);

      palette.PaletteTransitions.Should().NotBeNull();
      palette.PaletteTransitions.Length.Should().Be(7);
      
    }

    [Fact]
    public void Test_CutFillPalette_ChooseColour_NoTargetCCVColourInPVM()
    {
      var palette = new CutFillPalette();

      palette.Should().NotBeNull();
      
      var colors = new[] { Color.Red, Color.Crimson, Color.Yellow, Color.Blue, Color.SkyBlue, Color.DarkBlue };

      for (var i = 0; i < colors.Length; i++)
        palette.ChooseCutFillColour((float)(CUT_FILL_VALUE + i * VALUE_INCREMENT)).Should().Be(colors[i]);

      // Test centre values
      palette.ChooseCutFillColour(0.04F).Should().Be(Color.Green);
      palette.ChooseCutFillColour(0).Should().Be(Color.Green);
      palette.ChooseCutFillColour(-0.04F).Should().Be(Color.Green);
    }

  }
}
