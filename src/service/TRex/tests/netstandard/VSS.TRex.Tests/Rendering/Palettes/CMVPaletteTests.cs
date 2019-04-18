using System.Drawing;
using FluentAssertions;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client.Types;
using Xunit;

namespace VSS.TRex.Tests.Rendering.Palettes
{
  public class CMVPaletteTests
  {
    private const short MEASURED_CMV = 5;
    private const short TARGET_CMV = 90;
    private const short CMV_INCREMENT = 20;

    [Fact]
    public void Test_CMVPalette_Creation()
    {
      var palette = new CMVPalette();

      palette.Should().NotBeNull();

      palette.CMVPercentageRange.Min.Should().Be(80);
      palette.CMVPercentageRange.Max.Should().Be(120);
      palette.AbsoluteTargetCMV.Should().Be(70);
      palette.UseMachineTargetCMV.Should().Be(false);
      palette.DisplayTargetCCVColourInPVM.Should().Be(false);
      palette.DisplayDecoupledColourInPVM.Should().Be(false);
      palette.DefaultDecoupledCMVColour.Should().Be(Color.Black);
      palette.TargetCCVColour.Should().Be(Color.Blue);

      palette.PaletteTransitions.Should().NotBeNull();
      palette.PaletteTransitions.Length.Should().Be(5);
      palette.PaletteTransitions[0].Value.Should().Be(0);
      palette.PaletteTransitions[0].Color.Should().Be(Color.Green);
      palette.PaletteTransitions[1].Value.Should().Be(20);
      palette.PaletteTransitions[1].Color.Should().Be(Color.Yellow);
      palette.PaletteTransitions[2].Value.Should().Be(40);
      palette.PaletteTransitions[2].Color.Should().Be(Color.Olive);
      palette.PaletteTransitions[3].Value.Should().Be(60);
      palette.PaletteTransitions[3].Color.Should().Be(Color.Blue);
      palette.PaletteTransitions[4].Value.Should().Be(100);
      palette.PaletteTransitions[4].Color.Should().Be(Color.SkyBlue);
    }

    [Fact]
    public void Test_CMVPalette_ChooseColour_NoTargetCCVColourInPVM()
    {
      var palette = new CMVPalette();

      palette.Should().NotBeNull();
      palette.DisplayTargetCCVColourInPVM.Should().Be(false);

      var data = new SubGridCellPassDataCMVEntryRecord(MEASURED_CMV, TARGET_CMV, 0, 0);
      var colors = new []{ Color.Green, Color.Yellow, Color.Olive, Color.Blue, Color.Blue, Color.SkyBlue, Color.SkyBlue };

      for (var i = 0; i < colors.Length; i++)
      {
        data.MeasuredCMV = (short)(MEASURED_CMV + i * CMV_INCREMENT);
        palette.ChooseColour(data).Should().Be(colors[i]);
      }
    }

    [Fact]
    public void Test_CMVPalette_ChooseColour_TargetCCVColourInPVM_OverrideTarget()
    {
      var palette = new CMVPalette();

      palette.Should().NotBeNull();

      palette.DisplayTargetCCVColourInPVM = true;
      palette.DisplayTargetCCVColourInPVM.Should().Be(true);
      palette.UseMachineTargetCMV.Should().Be(false);
      palette.AbsoluteTargetCMV.Should().Be(70);

      var data = new SubGridCellPassDataCMVEntryRecord(MEASURED_CMV, TARGET_CMV, 0, 0);
      var colors = new[] { Color.Green, Color.Yellow, Color.Olive, palette.TargetCCVColour, Color.Blue, Color.SkyBlue, Color.SkyBlue };

      for (var i = 0; i < colors.Length; i++)
      {
        data.MeasuredCMV = (short)(MEASURED_CMV + i * CMV_INCREMENT);
        palette.ChooseColour(data).Should().Be(colors[i]);
      }
    }

    [Fact]
    public void Test_CMVPalette_ChooseColour_TargetCCVColourInPVM_MachineTarget()
    {
      var palette = new CMVPalette();

      palette.Should().NotBeNull();

      palette.DisplayTargetCCVColourInPVM = true;
      palette.DisplayTargetCCVColourInPVM.Should().Be(true);

      palette.UseMachineTargetCMV = true;
      palette.UseMachineTargetCMV.Should().Be(true);
      
      var data = new SubGridCellPassDataCMVEntryRecord(MEASURED_CMV, TARGET_CMV, 0, 0);
      var colors = new[] { Color.Green, Color.Yellow, Color.Olive, Color.Blue, palette.TargetCCVColour, palette.TargetCCVColour, Color.SkyBlue };

      for (var i = 0; i < colors.Length; i++)
      {
        data.MeasuredCMV = (short)(MEASURED_CMV + i * CMV_INCREMENT);
        palette.ChooseColour(data).Should().Be(colors[i]);
      }
    }
  }
}
