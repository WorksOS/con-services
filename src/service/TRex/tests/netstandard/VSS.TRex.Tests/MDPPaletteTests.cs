using VSS.TRex.Rendering.Palettes;
using Xunit;

namespace VSS.TRex.Tests
{
  public class MDPPaletteTests
  {
    [Fact]
    public void CanCreateMDPPalette()
    {
      var palette = new MDPPalette();
      Assert.NotNull(palette);
      Assert.NotNull(palette.PaletteTransitions);
    }

    [Fact]
    public void CanChooseMDPColor()
    {
      var palette = new MDPPalette();
      var transitions = palette.PaletteTransitions;
      var color = palette.ChooseColour(transitions[0].Value+1);
      Assert.Equal(transitions[0].Color, color);
    }
  }
}
