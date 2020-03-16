using System.Drawing;
using FluentAssertions;
using VSS.TRex.Rendering.Palettes;
using Xunit;

namespace VSS.TRex.Tests.Rendering.Palettes
{
  public class CCAPaletteTests
  {
    [Fact]
    public void Test_CCAPalette_Creation()
    {
      var palette = new CCAPalette();

      palette.Should().NotBeNull();

      palette.PaletteTransitions.Should().NotBeNull();
      palette.PaletteTransitions.Length.Should().Be(50);
      palette.PaletteTransitions[0].Value.Should().Be(0);
      palette.PaletteTransitions[0].Color.Should().Be(Color.Yellow);
      palette.PaletteTransitions[1].Value.Should().Be(1);
      palette.PaletteTransitions[1].Color.Should().Be(Color.Red);
      palette.PaletteTransitions[2].Value.Should().Be(2);
      palette.PaletteTransitions[2].Color.Should().Be(Color.Aqua);
      palette.PaletteTransitions[3].Value.Should().Be(3);
      palette.PaletteTransitions[3].Color.Should().Be(Color.Lime);
      palette.PaletteTransitions[4].Value.Should().Be(4);
      palette.PaletteTransitions[4].Color.Should().Be(System.Drawing.ColorTranslator.FromHtml("#FF8080"));
      palette.PaletteTransitions[5].Value.Should().Be(5);
      palette.PaletteTransitions[5].Color.Should().Be(System.Drawing.ColorTranslator.FromHtml("#91C8FF"));
      palette.PaletteTransitions[6].Value.Should().Be(6);
      palette.PaletteTransitions[6].Color.Should().Be(System.Drawing.ColorTranslator.FromHtml("#ACFDEB"));
      palette.PaletteTransitions[7].Value.Should().Be(7);
      palette.PaletteTransitions[7].Color.Should().Be(Color.Green);
      palette.PaletteTransitions[8].Value.Should().Be(8);
      palette.PaletteTransitions[8].Color.Should().Be(System.Drawing.ColorTranslator.FromHtml("#FFC0FF"));
      palette.PaletteTransitions[9].Value.Should().Be(9);
      palette.PaletteTransitions[9].Color.Should().Be(System.Drawing.ColorTranslator.FromHtml("#FFCB96"));
      palette.PaletteTransitions[10].Value.Should().Be(10);
      palette.PaletteTransitions[10].Color.Should().Be(System.Drawing.ColorTranslator.FromHtml("#6C8EB5"));
      palette.PaletteTransitions[11].Value.Should().Be(11);
      palette.PaletteTransitions[11].Color.Should().Be(System.Drawing.ColorTranslator.FromHtml("#80FFFF"));
      palette.PaletteTransitions[12].Value.Should().Be(12);
      palette.PaletteTransitions[12].Color.Should().Be(System.Drawing.ColorTranslator.FromHtml("#8080FF"));
      palette.PaletteTransitions[13].Value.Should().Be(13);
      palette.PaletteTransitions[13].Color.Should().Be(System.Drawing.ColorTranslator.FromHtml("#00FF80"));
      palette.PaletteTransitions[14].Value.Should().Be(14);
      palette.PaletteTransitions[14].Color.Should().Be(System.Drawing.ColorTranslator.FromHtml("#FF8000"));
      palette.PaletteTransitions[15].Value.Should().Be(15);
      palette.PaletteTransitions[15].Color.Should().Be(System.Drawing.ColorTranslator.FromHtml("#8000FF"));
      palette.PaletteTransitions[16].Value.Should().Be(16);
      palette.PaletteTransitions[16].Color.Should().Be(Color.Teal);
      palette.PaletteTransitions[17].Value.Should().Be(17);
      palette.PaletteTransitions[17].Color.Should().Be(System.Drawing.ColorTranslator.FromHtml("#C0C0FF"));
      palette.PaletteTransitions[18].Value.Should().Be(18);
      palette.PaletteTransitions[18].Color.Should().Be(System.Drawing.ColorTranslator.FromHtml("#FF80FF"));

      for (var i = 19; i <= 49; i++)
      {
        palette.PaletteTransitions[i].Value.Should().Be(i);
        palette.PaletteTransitions[i].Color.Should().Be(System.Drawing.ColorTranslator.FromHtml("#80FF00"));
      }
    }

    [Fact]
    public void Test_CCAPalette_ChooseColour()
    {
      var palette = new CCAPalette();

      palette.Should().NotBeNull();

      for (var i = 0; i < palette.PaletteTransitions.Length; i++)
        palette.ChooseColour(i).Should().Be(palette.PaletteTransitions[i].Color);
    }

  }
}
