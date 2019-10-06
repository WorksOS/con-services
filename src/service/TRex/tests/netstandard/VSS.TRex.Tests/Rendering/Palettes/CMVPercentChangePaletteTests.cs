using System.Drawing;
using FluentAssertions;
using VSS.TRex.Rendering.Palettes;
using Xunit;

namespace VSS.TRex.Tests.Rendering.Palettes
{
  public class CMVPercentChangePaletteTests
  {
    [Fact]
    public void Test_CMVPercentChangePalette_Creation()
    {
      var palette = new CMVPercentChangePalette();

      palette.Should().NotBeNull();

      palette.PaletteTransitions.Should().NotBeNull();
      palette.PaletteTransitions.Length.Should().Be(8);
      palette.PaletteTransitions[0].Value.Should().Be(short.MinValue);
      palette.PaletteTransitions[0].Color.Should().Be(ColorTranslator.FromHtml("#D50000"));
      palette.PaletteTransitions[1].Value.Should().Be(-50);
      palette.PaletteTransitions[1].Color.Should().Be(ColorTranslator.FromHtml("#E57373"));
      palette.PaletteTransitions[2].Value.Should().Be(-20);
      palette.PaletteTransitions[2].Color.Should().Be(ColorTranslator.FromHtml("#FFCDD2"));
      palette.PaletteTransitions[3].Value.Should().Be(-10);
      palette.PaletteTransitions[3].Color.Should().Be(ColorTranslator.FromHtml("#8BC34A"));
      palette.PaletteTransitions[4].Value.Should().Be(0);
      palette.PaletteTransitions[4].Color.Should().Be(ColorTranslator.FromHtml("#B3E5FC"));
      palette.PaletteTransitions[5].Value.Should().Be(10);
      palette.PaletteTransitions[5].Color.Should().Be(ColorTranslator.FromHtml("#4FC3F7"));
      palette.PaletteTransitions[6].Value.Should().Be(20);
      palette.PaletteTransitions[6].Color.Should().Be(ColorTranslator.FromHtml("#039BE5"));
      palette.PaletteTransitions[7].Value.Should().Be(50);
      palette.PaletteTransitions[7].Color.Should().Be(ColorTranslator.FromHtml("#01579B"));
    }

    [Fact]
    public void Test_CMVPercentChangePalette_ChooseColour()
    {
      var palette = new CMVPercentChangePalette();

      palette.Should().NotBeNull();

      palette.ChooseColour(-60).Should().Be(palette.PaletteTransitions[0].Color);
      palette.ChooseColour(-50).Should().Be(palette.PaletteTransitions[1].Color);
      palette.ChooseColour(-15).Should().Be(palette.PaletteTransitions[2].Color);
      palette.ChooseColour(0).Should().Be(palette.PaletteTransitions[4].Color);
      palette.ChooseColour(35).Should().Be(palette.PaletteTransitions[6].Color);
      palette.ChooseColour(100).Should().Be(palette.PaletteTransitions[7].Color);
    }
  }
}
