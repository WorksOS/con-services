using System.Drawing;
using FluentAssertions;
using VSS.TRex.Rendering.Palettes;
using Xunit;

namespace VSS.TRex.Tests.Rendering.Palettes
{
  public class PassCountPaletteTests
  {
    [Fact]
    public void Test_PassCountPalette_Creation()
    {
      var palette = new PassCountPalette();

      palette.Should().NotBeNull();

      palette.PaletteTransitions.Should().NotBeNull();
      palette.PaletteTransitions.Length.Should().Be(9);
      palette.PaletteTransitions[0].Value.Should().Be(1);
      palette.PaletteTransitions[0].Color.Should().Be(Color.DarkBlue);
      palette.PaletteTransitions[1].Value.Should().Be(2);
      palette.PaletteTransitions[1].Color.Should().Be(Color.DodgerBlue);
      palette.PaletteTransitions[2].Value.Should().Be(3);
      palette.PaletteTransitions[2].Color.Should().Be(Color.LightBlue);
      palette.PaletteTransitions[3].Value.Should().Be(4);
      palette.PaletteTransitions[3].Color.Should().Be(Color.YellowGreen);
      palette.PaletteTransitions[4].Value.Should().Be(5);
      palette.PaletteTransitions[4].Color.Should().Be(Color.DarkSeaGreen);
      palette.PaletteTransitions[5].Value.Should().Be(6);
      palette.PaletteTransitions[5].Color.Should().Be(Color.DarkGreen);
      palette.PaletteTransitions[6].Value.Should().Be(7);
      palette.PaletteTransitions[6].Color.Should().Be(Color.LightPink);
      palette.PaletteTransitions[7].Value.Should().Be(8);
      palette.PaletteTransitions[7].Color.Should().Be(Color.RosyBrown);
      palette.PaletteTransitions[8].Value.Should().Be(9);
      palette.PaletteTransitions[8].Color.Should().Be(Color.Brown);
    }

    [Fact]
    public void Test_PassCountPalette_ChooseColour()
    {
      const byte START_PASS_COUNT = 1;
      const byte PASS_COUNT_INCREMENT = 1;

      var palette = new PassCountPalette();

      palette.Should().NotBeNull();

      for (var i = 0; i < palette.PaletteTransitions.Length; i++)
        palette.ChooseColour(START_PASS_COUNT + i * PASS_COUNT_INCREMENT).Should().Be(palette.PaletteTransitions[i].Color);
    }
  }
}
