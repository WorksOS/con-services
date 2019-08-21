using System.Drawing;
using FluentAssertions;
using VSS.TRex.Rendering.Palettes;
using Xunit;

namespace VSS.TRex.Tests.Rendering.Palettes
{
  public class MDPPaletteTests
  {
    private const short MEASURED_MDP = 5;
    private const short TARGET_MDP = 90;
    private const short MDP_INCREMENT = 20;

    [Fact]
    public void Test_CMVPalette_Creation()
    {
      var palette = new MDPPalette();

      palette.Should().NotBeNull();

      palette.MDPPercentageRange.Min.Should().Be(75);
      palette.MDPPercentageRange.Max.Should().Be(110);
      palette.AbsoluteTargetMDP.Should().Be(50);
      palette.UseMachineTargetMDP.Should().Be(false);
      palette.DisplayTargetMDPColourInPVM.Should().Be(false);
      palette.TargetMDPColour.Should().Be(Color.Blue);

      palette.PaletteTransitions.Should().NotBeNull();
      palette.PaletteTransitions.Length.Should().Be(5);
      palette.PaletteTransitions[0].Value.Should().Be(0);
      palette.PaletteTransitions[0].Color.Should().Be(Color.Yellow);
      palette.PaletteTransitions[1].Value.Should().Be(20);
      palette.PaletteTransitions[1].Color.Should().Be(Color.Red);
      palette.PaletteTransitions[2].Value.Should().Be(40);
      palette.PaletteTransitions[2].Color.Should().Be(Color.Aqua);
      palette.PaletteTransitions[3].Value.Should().Be(75);
      palette.PaletteTransitions[3].Color.Should().Be(Color.Lime);
      palette.PaletteTransitions[4].Value.Should().Be(100);
      palette.PaletteTransitions[4].Color.Should().Be(ColorTranslator.FromHtml("#FF8080"));
    }

    [Fact]
    public void CanChooseMDPColor()
    {
      var palette = new MDPPalette();
      var transitions = palette.PaletteTransitions;
      var color = palette.ChooseColour(transitions[0].Value + 1);
      Assert.Equal(transitions[0].Color, color);
    }
  }
}
