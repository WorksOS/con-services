using System.Drawing;
using FluentAssertions;
using VSS.TRex.Rendering.Palettes;
using Xunit;

namespace VSS.TRex.Tests.Rendering.Palettes
{
  public class TemperaturePaletteTests
  {
    [Fact]
    public void Test_TemperaturePalette_Creation()
    {
      var palette = new TemperaturePalette();

      palette.Should().NotBeNull();

      palette.PaletteTransitions.Should().NotBeNull();
      palette.PaletteTransitions.Length.Should().Be(6);
      palette.PaletteTransitions[0].Value.Should().Be(0);
      palette.PaletteTransitions[0].Color.Should().Be(Color.Green);
      palette.PaletteTransitions[1].Value.Should().Be(200);
      palette.PaletteTransitions[1].Color.Should().Be(Color.Yellow);
      palette.PaletteTransitions[2].Value.Should().Be(400);
      palette.PaletteTransitions[2].Color.Should().Be(Color.Olive);
      palette.PaletteTransitions[3].Value.Should().Be(600);
      palette.PaletteTransitions[3].Color.Should().Be(Color.Blue);
      palette.PaletteTransitions[4].Value.Should().Be(800);
      palette.PaletteTransitions[4].Color.Should().Be(Color.SkyBlue);
      palette.PaletteTransitions[5].Value.Should().Be(1000);
      palette.PaletteTransitions[5].Color.Should().Be(Color.Red);
    }

    [Fact]
    public void Test_TemperaturePalette_ChooseColour()
    {
      const short START_TEMPERATURE = 100;
      const short TEMPERATURE_INCREMENT = 200;

      var palette = new TemperaturePalette();

      palette.Should().NotBeNull();

      for (var i = 0; i < palette.PaletteTransitions.Length; i++)
        palette.ChooseColour(START_TEMPERATURE + i * TEMPERATURE_INCREMENT).Should().Be(palette.PaletteTransitions[i].Color);
    }
  }
}
