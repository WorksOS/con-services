using System.Drawing;
using FluentAssertions;
using VSS.TRex.Rendering.Palettes;
using Xunit;

namespace VSS.TRex.Tests.Rendering.Palettes
{
  public class SpeedPaletteTests
  {
    [Fact]
    public void Test_SpeedPalette_Creation()
    {
      var palette = new SpeedPalette();

      palette.Should().NotBeNull();

      palette.PaletteTransitions.Should().NotBeNull();
      palette.PaletteTransitions.Length.Should().Be(5);
      palette.PaletteTransitions[0].Value.Should().Be(0);
      palette.PaletteTransitions[0].Color.Should().Be(Color.Green);
      palette.PaletteTransitions[1].Value.Should().Be(500);
      palette.PaletteTransitions[1].Color.Should().Be(Color.Yellow);
      palette.PaletteTransitions[2].Value.Should().Be(1000);
      palette.PaletteTransitions[2].Color.Should().Be(Color.Olive);
      palette.PaletteTransitions[3].Value.Should().Be(1500);
      palette.PaletteTransitions[3].Color.Should().Be(Color.Blue);
      palette.PaletteTransitions[4].Value.Should().Be(2000);
      palette.PaletteTransitions[4].Color.Should().Be(Color.SkyBlue);
    }

    [Fact]
    public void Test_SpeedPalette_ChooseColour()
    {
      const short START_SPEED = 250;
      const short SPEED_INCREMENT = 500;

      var palette = new SpeedPalette();

      palette.Should().NotBeNull();

      for (var i = 0; i < palette.PaletteTransitions.Length; i++)
        palette.ChooseColour(START_SPEED + i * SPEED_INCREMENT).Should().Be(palette.PaletteTransitions[i].Color);
    }
  }
}
